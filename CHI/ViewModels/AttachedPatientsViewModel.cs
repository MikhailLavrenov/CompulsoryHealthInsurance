using CHI.Infrastructure;
using CHI.Models;
using CHI.Services;
using CHI.Services.Common;
using CHI.Services.SRZ;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CHI.ViewModels
{
    class AttachedPatientsViewModel : DomainObject, IRegionMemberLifetime
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
        #endregion

        #region Конструкторы
        public AttachedPatientsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка прикрепленных пациентов из СРЗ";
            FileDate = DateTime.Today;

            ProcessFileCommand = new DelegateCommandAsync(ProcessFileExecute);
        }
        #endregion

        #region Методы
        private void ProcessFileExecute()
        {
            MainRegionService.ShowProgressBar("Проверка подключения к СРЗ.");

            if (!Settings.SrzConnectionIsValid)
                Settings.TestConnectionSRZ();

            if (!Settings.SrzConnectionIsValid && Settings.DownloadNewPatientsFile)
            {
                MainRegionService.HideProgressBar("Не удалось подключиться к СРЗ, проверьте настройки и доступность сайта. Возможно только подставить ФИО из БД в существующий файл.");
                return;
            }

            SleepMode.Deny();
            MainRegionService.ShowProgressBar("Выбор пути к файлу.");

            fileDialogService.DialogType = settings.DownloadNewPatientsFile ? FileDialogType.Save : FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.HideProgressBar("Отменено.");
                return;
            }

            settings.PatientsFilePath = fileDialogService.FileName;

            var dbLoadingTask = Task.Run(() =>
            {
                var dbContext = new AppDBContext();
                dbContext.Patients.Load();
                return dbContext;
            });

            if (Settings.DownloadNewPatientsFile)
            {
                MainRegionService.ShowProgressBar("Скачивание файла.");

                var service = new SRZService(Settings.SrzAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);

                var credential = Settings.SrzCredentials.First();
                service.Authorize(credential);
                service.GetPatientsFile(Settings.PatientsFilePath, FileDate);
            }

            MainRegionService.ShowProgressBar("Подстановка ФИО в файл.");

            var db = dbLoadingTask.ConfigureAwait(false).GetAwaiter().GetResult();

            using var file = new AttachedPatientsFileService(Settings.PatientsFilePath, Settings.ColumnProperties);
            file.InsertPatientsWithFullName(db.Patients.ToList());

            var resultReport = new StringBuilder();

            if (Settings.SrzConnectionIsValid)
            {
                var unknownInsuaranceNumbers = file.GetInsuranceNumberOfPatientsWithoutFullName().Take((int)Settings.SrzRequestsLimit).ToList();

                MainRegionService.ShowProgressBar("Поиск ФИО в СРЗ.");
                var foundPatients = GetPatients(unknownInsuaranceNumbers);

                resultReport.Append($"Запрошено пациентов в СРЗ: {foundPatients.Count()}, лимит {Settings.SrzRequestsLimit}. ");
                MainRegionService.ShowProgressBar("Подстановка ФИО в файл.");
                file.InsertPatientsWithFullName(foundPatients);

                MainRegionService.ShowProgressBar("Добавление ФИО в локальную базу данных.");
                var duplicateInsuranceNumbers = new HashSet<string>(foundPatients.Select(x => x.InsuranceNumber).ToList());
                var duplicatePatients = db.Patients.Where(x => duplicateInsuranceNumbers.Contains(x.InsuranceNumber)).ToArray();

                db.Patients.RemoveRange(duplicatePatients);
                db.SaveChanges();

                db.Patients.AddRange(foundPatients);
                db.SaveChanges();
            }
            else
                resultReport.Append("ФИО подставлены только из локальной БД. ");

            var unknownPatients = file.GetInsuranceNumberOfPatientsWithoutFullName();

            if (Settings.FormatPatientsFile && unknownPatients.Count == 0)
            {
                MainRegionService.ShowProgressBar("Форматирование файла.");
                file.Format();
            }

            MainRegionService.ShowProgressBar("Сохранение файла.");
            file.Save();

            if (!Settings.SrzConnectionIsValid && unknownPatients.Count != 0)
                resultReport.Append("Не удалось подключиться к СРЗ, проверьте настройки и доступность сайта. ");

            if (unknownPatients.Count == 0)
                resultReport.Append($"Файл готов, все ФИО найдены.");
            else
                resultReport.Append($"Файл не готов, осталось найти {unknownPatients.Count} ФИО.");

            SleepMode.Deny();
            MainRegionService.HideProgressBar(resultReport.ToString());
        }
        //запускает многопоточно запросы к сайту для поиска пациентов
        private Patient[] GetPatients(List<string> insuranceNumbers)
        {
            int counter = 0;
            int threadsLimit = insuranceNumbers.Count > Settings.SrzThreadsLimit ? Settings.SrzThreadsLimit : insuranceNumbers.Count;
            var requestsLimit = insuranceNumbers.Count > Settings.SrzRequestsLimit ? Settings.SrzRequestsLimit : (uint)insuranceNumbers.Count;

            var circularList = new CircularList<Credential>(Settings.SrzCredentials);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<SRZService>[threadsLimit];
            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => (SRZService)null);

            for (int i = 0; i < requestsLimit; i++)
            {
                var insuranceNumber = insuranceNumbers[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    var credential = (Credential)service?.Credential;

                    if (credential == null)
                    {
                        service = new SRZService(Settings.SrzAddress, Settings.UseProxy, Settings.ProxyAddress, Settings.ProxyPort);

                        service.Authorize(circularList.GetNext());
                    }

                    var patient = service.GetPatient(insuranceNumber);

                    if (patient != null)
                    {
                        verifiedPatients.Add(patient);
                        Interlocked.Increment(ref counter);
                        MainRegionService.ShowProgressBar($"Запрошено ФИО в СРЗ: {verifiedPatients.Count()} из {insuranceNumbers.Count}.");
                    }

                    return service;
                });
            }
            Task.WaitAll(tasks);

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

            return verifiedPatients.ToArray();
        }
        #endregion
    }
}
