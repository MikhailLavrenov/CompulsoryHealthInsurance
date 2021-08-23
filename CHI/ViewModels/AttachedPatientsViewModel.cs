using CHI.Infrastructure;
using CHI.Models;
using CHI.Services;
using CHI.Services.SRZ;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.ViewModels
{
    class AttachedPatientsViewModel : DomainObject, IRegionMemberLifetime
    {
        Settings settings;
        DateTime fileDate;
        readonly IFileDialogService fileDialogService;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        public DelegateCommandAsync ProcessFileCommand { get; }


        public AttachedPatientsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Загрузка прикрепленных пациентов из СРЗ";
            FileDate = DateTime.Today;

            ProcessFileCommand = new DelegateCommandAsync(ProcessFileExecute);
        }


        async void ProcessFileExecute()
        {
            MainRegionService.ShowProgressBar("Проверка подключения к СРЗ.");

            if (!Settings.SrzConnectionIsValid)
                await Settings.TestConnectionSRZAsync();

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
                await service.AuthorizeAsync(credential);
                await service.GetPatientsFileAsync(Settings.PatientsFilePath, FileDate);
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
                var parallelSrzService = new ParallelSRZService(Settings.SrzAddress, Settings.SrzCredentials.First(), Settings.SrzThreadsLimit);
                if (Settings.UseProxy)
                    parallelSrzService.UseProxy(Settings.ProxyAddress, Settings.ProxyPort);

                var foundPatients = await parallelSrzService.GetPatientsAsync(unknownInsuaranceNumbers);

                resultReport.Append($"Запрошено пациентов в СРЗ: {foundPatients.Count}, лимит {Settings.SrzRequestsLimit}. ");
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
    }
}
