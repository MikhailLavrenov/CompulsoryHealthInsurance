using CHI.Infrastructure;
using CHI.Models;
using CHI.Services;
using CHI.Services.MedicalExaminations;
using CHI.Settings;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHI.ViewModels
{
    class ExaminationsViewModel : DomainObject, IRegionMemberLifetime
    {
        List<(PatientExaminations PatientExaminations, bool IsLoaded, string Error)> result;
        bool showErrors;
        readonly IFileDialogService fileDialogService;


        public IMainRegionService MainRegionService { get; set; }
        public ILicenseManager LicenseManager { get; set; }
        public bool KeepAlive { get => false; }
        public bool ShowErrors { get => showErrors; set => SetProperty(ref showErrors, value); }
        public List<(PatientExaminations PatientExaminations, bool IsLoaded, string Error)> Result { get => result; set => SetProperty(ref result, value); }
        public AppSettings Settings { get; set; }
        public DelegateCommandAsync ExportExaminationsCommand { get; }


        public ExaminationsViewModel(AppSettings settings, IMainRegionService mainRegionService, IFileDialogService fileDialogService, ILicenseManager licenseManager)
        {
            Settings = settings;
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;
            LicenseManager = licenseManager;

            Result = new List<(PatientExaminations, bool, string)>();

            ShowErrors = false;
            MainRegionService.Header = "Загрузка осмотров на портал Диспансеризации";

            ExportExaminationsCommand = new DelegateCommandAsync(ExportExaminationsExecuteAsync);
        }


        async void ExportExaminationsExecuteAsync()
        {
            Result?.Clear();
            ShowErrors = false;

            if (!Settings.MedicalExaminations.ConnectionIsValid)
            {
                MainRegionService.ShowProgressBar("Проверка настроек.");

                await Settings.TestConnectionExaminationsAsync();
                if (!Settings.MedicalExaminations.ConnectionIsValid)
                {
                    MainRegionService.HideProgressBar("Не удалось подключиться к web-сервису.");
                    return;
                }
            }
            SleepMode.Deny();

            MainRegionService.ShowProgressBar("Выбор файлов.");

            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = Settings.MedicalExaminations.FileDirectory;
            fileDialogService.MiltiSelect = true;
            fileDialogService.Filter = "Zip files (*.zip)|*.zip|Xml files (*.xml)|*.xml";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.HideProgressBar("Отменено.");
                return;
            }

            Settings.MedicalExaminations.FileDirectory = Path.GetDirectoryName(fileDialogService.FileNames.FirstOrDefault());

            MainRegionService.ShowProgressBar("Чтение файлов.");

            var registers = new MedExamsBillsRegisterService();
            var fileFilter = $"{Settings.MedicalExaminations.PatientsFileNames},{Settings.MedicalExaminations.CasesFileNames}".Split(',', StringSplitOptions.TrimEntries);
            registers.XmlFileNameStartsWithFilter = fileFilter.ToList();

            var patientsExaminations = registers.GetPatientExaminationsList(fileDialogService.FileNames);

            var maxDate1 = patientsExaminations.Max(x => x.Stage1?.EndDate);
            var maxDate2 = patientsExaminations.Max(x => x.Stage2?.EndDate);
            var maxDate = maxDate1 > maxDate2 ? maxDate1 : maxDate2;

            var license = LicenseManager.ActiveLicense;

            if (!(license.ExaminationsUnlimited || license.ExaminationsFomsCodeMO == Settings.MedicalExaminations.FomsCodeMO || license.ExaminationsMaxDate > maxDate))
            {
                MainRegionService.HideProgressBar("Отменено, ограничение лицензии.");
                return;
            }

            MainRegionService.ShowProgressBar($"Загрузка осмотров. Всего пациентов: {patientsExaminations.Count}.");

            var parallelSerivce = new ParallelExaminationsService(Settings.MedicalExaminations.Address, Settings.MedicalExaminations.Credential, Settings.MedicalExaminations.MaxDegreeOfParallelism);
            if (Settings.Common.UseProxy)
                parallelSerivce.UseProxy(Settings.Common.ProxyAddress, Settings.Common.ProxyPort);
            Result = await parallelSerivce.AddExaminationsAsync(patientsExaminations);

            Result.OrderBy(x => x.IsLoaded)
                .ThenBy(x => x.PatientExaminations.Kind)
                .ThenBy(x => x.PatientExaminations.Year)
                .ToList();

            if (Result?.Count > 0)
                ShowErrors = true;

            SleepMode.Allow();
            MainRegionService.HideProgressBar("Завершено.");
        }

        //private List<Tuple<PatientExaminations, bool, string>> AddExaminationsParallel(List<PatientExaminations> patientsExaminations)
        //{
        //    var threadsLimit = patientsExaminations.Count > Settings.ExaminationsThreadsLimit ? Settings.ExaminationsThreadsLimit : patientsExaminations.Count;
        //    var result = new ConcurrentBag<Tuple<PatientExaminations, bool, string>>();
        //    var tasks = new Task<ExaminationService>[threadsLimit];
        //    var counter = 0;
        //    //задержка потока перед обращением к веб-серверу, увеличивается при росте неудачных попыток, и уменьшается при росте удачных
        //    var sleepTime = 0;

        //    for (int i = 0; i < threadsLimit; i++)
        //        tasks[i] = Task.Run(() => (ExaminationService)null);

        //    for (int i = 0; i < patientsExaminations.Count; i++)
        //    {
        //        var patientExaminations = patientsExaminations[i];
        //        var index = Task.WaitAny(tasks);
        //        tasks[index] = tasks[index].ContinueWith(async (task) =>
        //        {
        //            var service = await task;
        //            string error = string.Empty;
        //            bool isSuccessful = true;

        //            //3 попытки на загрузку осмотра, т.к. иногда веб-сервер обрывает сессии
        //            for (int j = 1; j < 4; j++)
        //            {
        //                if (j != 1)
        //                    Interlocked.Add(ref sleepTime, 5000);

        //                Thread.Sleep(sleepTime);

        //                try
        //                {
        //                    if (service == null)
        //                    {
        //                        service = new ExaminationService(Settings.ExaminationsAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);
        //                        await service.AuthorizeAsync(settings.ExaminationsCredentials.First());
        //                    }

        //                    await service.AddPatientExaminationsAsync(patientExaminations);
        //                    error = string.Empty;
        //                    isSuccessful = true;

        //                    if (sleepTime != 0)
        //                    {
        //                        //универсальный InterLocked-паттерн, потокобезопасно уменьшает sleepRate если он положительный
        //                        int initial, desired;

        //                        do
        //                        {
        //                            initial = sleepTime;
        //                            desired = initial;
        //                            if (desired >= 1000)
        //                                desired -= 1000;
        //                            else if (desired > 0)
        //                                desired = 0;
        //                        }
        //                        while (initial != Interlocked.CompareExchange(ref sleepTime, desired, initial));
        //                    }

        //                    break;
        //                }
        //                catch (HttpRequestException ex)
        //                {
        //                    error = ex.Message;
        //                    isSuccessful = false;
        //                    service = null;
        //                }
        //                catch (InvalidOperationException ex)
        //                {
        //                    error = ex.Message;
        //                    isSuccessful = false;
        //                }
        //                catch (WebServiceOperationException ex)
        //                {
        //                    error = ex.Message;
        //                    isSuccessful = false;
        //                    break;
        //                }
        //            }

        //            result.Add(new Tuple<PatientExaminations, bool, string>(patientExaminations, isSuccessful, error));
        //            Interlocked.Increment(ref counter);
        //            MainRegionService.ShowProgressBar($"Загрузка осмотров. Обработано пациентов: {counter} из {patientsExaminations.Count}.");

        //            return service;
        //        });
        //    }

        //    for (int i = 0; i < tasks.Length; i++)
        //    {
        //        var index = Task.WaitAny(tasks);

        //        tasks[index] = tasks[index].ContinueWith(async (task) =>
        //        {
        //            var service = await task;
        //            await service?.LogoutAsync();
        //            return service;
        //        });
        //    }

        //    Task.WaitAll(tasks);

        //    return result.ToList();
        //}
    }
}
