using CHI.Infrastructure;
using CHI.Services;
using CHI.Services.SRZ;
using CHI.Settings;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.ViewModels
{
    public class AttachedPatientsViewModel : DomainObject, IRegionMemberLifetime
    {
        DateTime fileDate;
        readonly IFileDialogService fileDialogService;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        public DelegateCommandAsync ProcessFileCommand { get; }


        public AttachedPatientsViewModel(AppSettings settings, IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            Settings = settings;
            this.fileDialogService = fileDialogService;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Загрузка прикрепленных пациентов из СРЗ";
            FileDate = DateTime.Today;

            ProcessFileCommand = new DelegateCommandAsync(ProcessFileExecute);
        }


        async void ProcessFileExecute()
        {
            MainRegionService.ShowProgressBar("Проверка подключения к СРЗ.");

            if (!Settings.Srz.ConnectionIsValid)
                await Settings.TestConnectionSRZAsync();

            if (!Settings.Srz.ConnectionIsValid && Settings.Srz.DownloadNewPatientsFile)
            {
                MainRegionService.HideProgressBar("Не удалось подключиться к СРЗ, проверьте настройки и доступность сайта. Возможно только подставить ФИО из БД в существующий файл.");
                return;
            }

            SleepMode.Deny();
            MainRegionService.ShowProgressBar("Выбор пути к файлу.");

            fileDialogService.DialogType = Settings.Srz.DownloadNewPatientsFile ? FileDialogType.Save : FileDialogType.Open;
            fileDialogService.FileName = Settings.AttachedPatientsFile.Path;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
            {
                MainRegionService.HideProgressBar("Отменено.");
                return;
            }

            Settings.AttachedPatientsFile.Path = fileDialogService.FileName;

            var dbLoadingTask = Task.Run(() =>
            {
                var dbContext = new AppDBContext(Settings.Common.SqlServer, Settings.Common.SqlDatabase, Settings.Common.SqlLogin, Settings.Common.SqlPassword);
                dbContext.Patients.Load();
                return dbContext;
            });

            if (Settings.Srz.DownloadNewPatientsFile)
            {
                MainRegionService.ShowProgressBar("Скачивание файла.");

                var service = new SRZService(Settings.Srz.Address, Settings.Common.UseProxy, Settings.Common.ProxyAddress, Settings.Common.ProxyPort);

                await service.AuthorizeAsync(Settings.Srz.Credential);
                await service.GetPatientsFileAsync(Settings.AttachedPatientsFile.Path, FileDate);
            }

            MainRegionService.ShowProgressBar("Подстановка ФИО в файл.");

            var db = dbLoadingTask.ConfigureAwait(false).GetAwaiter().GetResult();

            using var file = new AttachedPatientsFileService(Settings.AttachedPatientsFile.Path, Settings.AttachedPatientsFile.ColumnProperties);
            file.InsertPatientsWithFullName(db.Patients.ToList());

            var resultReport = new StringBuilder();

            if (Settings.Srz.ConnectionIsValid)
            {
                var unknownInsuaranceNumbers = file.GetInsuranceNumberOfPatientsWithoutFullName().Take((int)Settings.Srz.RequestsLimit).ToList();

                MainRegionService.ShowProgressBar("Поиск ФИО в СРЗ.");
                var parallelSrzService = new ParallelSRZService(Settings.Srz.Address, Settings.Srz.Credential, Settings.Srz.MaxDegreeOfParallelism);
                if (Settings.Common.UseProxy)
                    parallelSrzService.UseProxy(Settings.Common.ProxyAddress, Settings.Common.ProxyPort);
                parallelSrzService.ProgressChanged += n => MainRegionService.ShowProgressBar($"В СРЗ запрощено {n} ФИО из {unknownInsuaranceNumbers.Count}.");
                var foundPatients = await parallelSrzService.GetPatientsAsync(unknownInsuaranceNumbers);

                resultReport.Append($"Запрошено пациентов в СРЗ: {foundPatients.Count}, лимит {Settings.Srz.RequestsLimit}. ");
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

            if (Settings.AttachedPatientsFile.ApplyFormat && unknownPatients.Count == 0)
            {
                MainRegionService.ShowProgressBar("Форматирование файла.");
                file.Format();
            }

            MainRegionService.ShowProgressBar("Сохранение файла.");
            file.Save();

            if (!Settings.Srz.ConnectionIsValid && unknownPatients.Count != 0)
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
