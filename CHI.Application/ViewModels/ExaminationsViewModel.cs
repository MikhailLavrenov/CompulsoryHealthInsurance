using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.Common;
using CHI.Services.MedicalExaminations;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CHI.Application.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private List<Tuple<PatientExaminations, bool, string>> result;
        private bool showErrors;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public ILicenseManager LicenseManager { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public List<Tuple<PatientExaminations, bool, string>> Result { get => result; set => SetProperty(ref result, value); }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync ExportExaminationsCommand { get; }
        #endregion

        #region Конструкторы
        public ExaminationsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, ILicenseManager licenseManager)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;
            LicenseManager = licenseManager;

            Result = new List<Tuple<PatientExaminations, bool, string>>();

            ShowErrors = false;
            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка периодических осмотров на портал";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecute);


            //var examination1Stage = new Examination
            //{
            //    BeginDate = new DateTime(2019, 10, 10),
            //    EndDate = new DateTime(2019, 10, 15),
            //    HealthGroup = HealthGroup.ThirdA,
            //    Referral = Referral.LocalClinic
            //};
            //var examination2Stage = new Examination
            //{
            //    BeginDate = new DateTime(2019, 10, 20),
            //    EndDate = new DateTime(2019, 10, 25),
            //    HealthGroup = HealthGroup.ThirdB,
            //    Referral = Referral.AnotherClinic
            //};

            //ShowErrors = true;
            //var pe = new PatientExaminations("2751530822000157", 2019, ExaminationKind.Dispanserizacia1)
            //{
            //    Stage1 = examination1Stage,
            //    Stage2 = examination2Stage
            //};
            
            //Result = new List<Tuple<PatientExaminations, bool, string>> {
            //    new Tuple<PatientExaminations, bool,string>(pe, true, ""),
            //    new Tuple<PatientExaminations,bool, string>(pe, true, ""),
            //    new Tuple<PatientExaminations,bool, string>(pe, false, "Какая то надпись"),
            //    new Tuple<PatientExaminations, bool,string>(pe, false, "Еще одна надпись"),
            //};

            //for (int i = 0; i < 1000; i++)
            //{
            //    var item = new Tuple<PatientExaminations, bool, string>(pe, false, "Какая то надпись");
            //    Result.Add(item);
            //}
        }
        #endregion

        #region Методы
        private void ExportExaminationsExecute()
        {
            Result?.Clear();
            ShowErrors = false;

            if (!Settings.ExaminationsConnectionIsValid)
            {
                MainRegionService.SetBusyStatus("Проверка настроек.");

                Settings.TestConnectionExaminations();
                if (!Settings.ExaminationsConnectionIsValid)
                {
                    MainRegionService.SetCompleteStatus("Не удалось подключиться к web-сервису.");
                    return;
                }
            }
            SleepMode.PreventOn();

            MainRegionService.SetBusyStatus("Выбор файлов.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = Settings.ExaminationsFileDirectory;
            fileDialogService.MiltiSelect = true;
            fileDialogService.Filter = "Zip files (*.zip)|*.zip|Xml files (*.xml)|*.xml";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.SetCompleteStatus("Отменено.");
                return;
            }

            Settings.ExaminationsFileDirectory =Path.GetDirectoryName(fileDialogService.FileNames.FirstOrDefault());

            MainRegionService.SetBusyStatus("Чтение файлов.");

            var registers = new BillsRegisterService(fileDialogService.FileNames);
            var patientsFileNames = Settings.PatientFileNames.Split(',');
            var examinationFileNames = Settings.ExaminationFileNames.Split(',');

            for (int i = 0; i < patientsFileNames.Length; i++)
                patientsFileNames[i] = patientsFileNames[i].Trim();
            for (int i = 0; i < examinationFileNames.Length; i++)
                examinationFileNames[i] = examinationFileNames[i].Trim();

            var patientsExaminations = registers.GetPatientsExaminations(examinationFileNames, patientsFileNames);

            var maxDate1 = patientsExaminations.Max(x => x.Stage1?.EndDate);
            var maxDate2 = patientsExaminations.Max(x => x.Stage2?.EndDate);
            var maxDate = maxDate1 > maxDate2 ? maxDate1 : maxDate2;

            var license = LicenseManager.ActiveLicense;

            if (!(license.ExaminationsUnlimited || license.ExaminationsFomsCodeMO == Settings.FomsCodeMO || license.ExaminationsMaxDate > maxDate))
            {
                MainRegionService.SetCompleteStatus("Отменено, ограничение лицензии.");
                return;
            }

            MainRegionService.SetBusyStatus($"Загрузка осмотров. Всего пациентов: {patientsExaminations.Count}.");

            Result = AddExaminationsParallel(patientsExaminations)
                .OrderBy(x => x.Item2)
                .ThenBy(x => x.Item1.Kind)
                .ThenBy(x => x.Item1.Year)
                .ToList();

            if (Result?.Count > 0)
                ShowErrors = true;

            SleepMode.PreventOff();
            MainRegionService.SetCompleteStatus("Завершено.");
        }
        /// <summary>
        /// Загружает осмотры на портал диспансеризации. В случае возникновения исключений при загрузке осмотра - предпринимает несколько попыток.
        /// </summary>
        /// <param name="patientsExaminations">Список профилактических осмотров пациентов.</param>
        /// <returns>Список кортежей состоящий из PatientExaminations, флага успешной загрузки (true-успешно, false-иначе), строки с сообщением об ошибке.</returns>
        private List<Tuple<PatientExaminations, bool, string>> AddExaminationsParallel(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = patientsExaminations.Count > Settings.ExaminationsThreadsLimit ? Settings.ExaminationsThreadsLimit : patientsExaminations.Count;
            var circularList = new CircularList<Credential>(Settings.ExaminationsCredentials);
            var result = new ConcurrentBag<Tuple<PatientExaminations, bool, string>>();
            var tasks = new Task<ExaminationService>[threadsLimit];
            var counter = 0;
            //задержка потока перед обращением к веб-серверу, увеличивается при росте неудачных попыток, и уменьшается при росте удачных
            var sleepTime = 0;

            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => (ExaminationService)null);

            for (int i = 0; i < patientsExaminations.Count; i++)
            {
                var patientExaminations = patientsExaminations[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    string error = string.Empty;
                    bool isSuccessful = true;

                    //3 попытки на загрузку осмотра, т.к. иногда веб-сервер обрывает сессии
                    for (int j = 1; j < 4; j++)
                    {
                        if (j != 1)
                            Interlocked.Add(ref sleepTime, 5000);

                        Thread.Sleep(sleepTime);

                        try
                        {
                            if (service == null)
                            {
                                service = new ExaminationService(Settings.ExaminationsAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);
                                service.Authorize(circularList.GetNext());
                            }

                            service.AddPatientExaminations(patientExaminations);
                            error = string.Empty;
                            isSuccessful = true;

                            if (sleepTime != 0)
                            {
                                //универсальный InterLocked-паттерн, потокобезопасно уменьшает sleepRate если он положительный
                                int initial, desired;

                                do 
                                {
                                    initial = sleepTime;
                                    desired = initial;
                                    if (desired >= 1000)
                                        desired -= 1000;
                                    else if (desired > 0)
                                        desired = 0;
                                }
                                while (initial != Interlocked.CompareExchange(ref sleepTime, desired, initial));
                            }

                            break;
                        }
                        catch (HttpRequestException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                            service = null;
                        }
                        catch (InvalidOperationException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                        }
                        catch (WebServiceOperationException ex)
                        {
                            error = ex.Message;
                            isSuccessful = false;
                            break;
                        }
                    }

                    result.Add(new Tuple<PatientExaminations, bool, string>(patientExaminations, isSuccessful, error));
                    Interlocked.Increment(ref counter);
                    MainRegionService.SetBusyStatus($"Загрузка осмотров. Обработано пациентов: {counter} из {patientsExaminations.Count}.");

                    return service;
                });
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                var index = Task.WaitAny(tasks);

                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    service?.Logout();
                    return service;
                });
            }

            Task.WaitAll(tasks);

            return result.ToList();
        }
        #endregion



    }
}
