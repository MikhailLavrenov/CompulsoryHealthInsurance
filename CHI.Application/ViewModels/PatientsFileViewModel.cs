using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.AttachedPatients;
using CHI.Services.Common;
using CHI.Services.SRZ;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.Application.ViewModels
{
    class PatientsFileViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private DateTime fileDate;

        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        public DelegateCommandAsync ProcessFileCommand { get; }
        public DelegateCommand ShowFileDialogCommand { get; }

        #endregion

        #region Конструкторы
        public PatientsFileViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Файл прикрепленных пациентов";
            FileDate = DateTime.Today;

            ProcessFileCommand = new DelegateCommandAsync(ProcessFileExecute, ProcessFileCanExecute);
            ShowFileDialogCommand = new DelegateCommand(ShowFileDialogExecute);
        }
        #endregion

        #region Методы
        private void ShowFileDialogExecute()
        {
            fileDialogService.DialogType = settings.DownloadNewPatientsFile ? FileDialogType.Save : FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() == true)
                settings.PatientsFilePath = fileDialogService.FileName;
        }
        private void ProcessFileExecute()
        {
            MainRegionService.SetBusyStatus("Проверка подключения к СРЗ.");
            Settings.TestConnection();

            if (Settings.DownloadNewPatientsFile)
            {
                if (Settings.ConnectionIsValid == false)
                {
                    MainRegionService.SetCompleteStatus("Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта. Без подключения к СРЗ возможно только подставить ФИО из кэша в существующий файл.");
                    return;
                }

                MainRegionService.SetBusyStatus("Загрузка файла из СРЗ.");

                var service = new SRZService(Settings.SRZAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);

                var credential = Settings.Credentials.First(x => x.RequestsLimit > 0);
                service.TryAuthorize(credential);
                service.GetPatientsFile(Settings.PatientsFilePath, FileDate);
            }

            MainRegionService.SetBusyStatus("Подстановка ФИО из кэша.");
            var db = new Models.Database();
            var file = new PatientsFileService();

            db.Patients.Load();
            file.Open(Settings.PatientsFilePath, Settings.ColumnProperties);
            file.SetFullNames(db.Patients.ToList());

            string resultReport;

            if (Settings.ConnectionIsValid)
            {
                MainRegionService.SetBusyStatus("Поиск пациентов без ФИО в файле.");
                var limitCount = Settings.Credentials.Sum(x => x.RequestsLimit);
                var unknownInsuaranceNumbers = file.GetUnknownInsuaranceNumbers(limitCount);

                MainRegionService.SetBusyStatus("Поиск ФИО в СРЗ.");
                var verifiedPatients = GetPatients(unknownInsuaranceNumbers);

                MainRegionService.SetBusyStatus("Подстановка в файл ФИО найденных в СРЗ.");
                file.SetFullNames(verifiedPatients);

                MainRegionService.SetBusyStatus("Ожидайте. Добавление в кэш ФИО найденных в СРЗ.");
                var duplicateInsuranceNumber = new HashSet<string>(verifiedPatients.Select(x => x.InsuranceNumber));
                var duplicatePatients = db.Patients.Where(x => duplicateInsuranceNumber.Contains(x.InsuranceNumber)).ToArray();
                db.Patients.RemoveRange(duplicatePatients);
                db.SaveChanges();

                db.Patients.AddRange(verifiedPatients);
                db.SaveChanges();

                resultReport = $"В СРЗ запрошено {verifiedPatients.Count()} человек, лимит {limitCount}. ";
            }
            else
                resultReport = $"ФИО подставлены только из кэша.  Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта.";

            MainRegionService.SetBusyStatus("Подсчет человек без ФИО.");
            var unknownPatients = file.GetUnknownInsuaranceNumbers(int.MaxValue);

            if (unknownPatients.Count == 0)
            {
                MainRegionService.SetBusyStatus("Форматирование файла.");
                file.Format();
            }

            MainRegionService.SetBusyStatus("Сохранение изменений.");
            file.Save();

            if (unknownPatients.Count == 0)
                MainRegionService.SetCompleteStatus($"{ resultReport} Файл готов, найдены все ФИО.");
            else
                MainRegionService.SetCompleteStatus($"{ resultReport} Файл не готов, осталось найти {unknownPatients.Count} ФИО.");
        }
        private bool ProcessFileCanExecute()
        {
            if (Settings.DownloadNewPatientsFile == false && File.Exists(Settings.PatientsFilePath) == false)
                return false;
            else
                return true;
        }
        //запускает многопоточно запросы к сайту для поиска пациентов
        private Patient[] GetPatients(List<string> insuranceNumbers)
        {
            int threadsLimit = Settings.ThreadsLimit;
            if (insuranceNumbers.Count < threadsLimit)
                threadsLimit = insuranceNumbers.Count;

            var dictionary = new Dictionary<Credential, uint>();
            foreach (var credential in Settings.Credentials)
                dictionary.Add(credential, credential.RequestsLimit);

            var circularRestrictedList = new CircularListWithCounter<Credential>(dictionary);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<SRZService>[threadsLimit];
            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => { return (SRZService)null; });

            for (int i = 0; i < insuranceNumbers.Count; i++)
            {
                var insuranceNumber = insuranceNumbers[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    var credential = (Credential)service?.Credential;

                    if (credential == null || !circularRestrictedList.TryReserve(credential))
                    {
                        service?.Logout();

                        while (true)
                        {
                            if (!circularRestrictedList.TryGetNext(out credential))
                                return null;

                            if (circularRestrictedList.TryReserve(credential))
                            {
                                service = new SRZService(Settings.SRZAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);

                                if (service.TryAuthorize(credential))
                                    break;
                            }
                        }
                    }

                    var patient = service.GetPatient(insuranceNumber);

                    if (patient != null)
                        verifiedPatients.Add(patient);

                    return service;
                });
            }
            Task.WaitAll(tasks);

            return verifiedPatients.ToArray();
        }
        #endregion
    }
}
