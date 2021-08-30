using CHI.Infrastructure;
using CHI.Services;
using CHI.Settings;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.ViewModels
{
    public class AttachedPatientsStorageSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        readonly IFileDialogService fileDialogService;
        AppSettings settings;
        IMainRegionService mainRegionService;
        string patientsCount = "Вычисляется...";


        public string PatientsCount { get => patientsCount; set => SetProperty(ref patientsCount, value); }
        public bool KeepAlive { get => false; }
        public DelegateCommandAsync ImportPatientsCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }
        public DelegateCommand ClearDatabaseCommand { get; }


        public AttachedPatientsStorageSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {

            this.fileDialogService = fileDialogService;
            this.settings = settings;
            this.mainRegionService = mainRegionService;

            this.mainRegionService.Header = "База данных прикрепленных пациентов";

            Task.Run(() => PatientsCount = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB).Patients.Count().ToString());

            ImportPatientsCommand = new DelegateCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new DelegateCommand(ClearDatabaseExecute);
        }


        private void ImportPatientsExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var importFilePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBar("Открытие файла.");

            var importReader = new ImportPatientsFileService(importFilePath);
            var newPatients = importReader.GetPatients();

            mainRegionService.ShowProgressBar("Проверка значений.");
            var db = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);
            db.Patients.Load();

            var existenInsuaranceNumbers = new HashSet<string>(db.Patients.Select(x => x.InsuranceNumber));
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            mainRegionService.ShowProgressBar("Сохранение в локальную базу данных.");
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            PatientsCount = db.Patients.Count().ToString();

            mainRegionService.HideProgressBar($"В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count} новых человек(а).");
        }

        private void SaveExampleExecute()
        {
            mainRegionService.ShowProgressBar("Выбор пути");

            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример для загрузки ФИО";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            mainRegionService.ShowProgressBar("Сохранение файла");

            ImportPatientsFileService.SaveExample(saveExampleFilePath);

            mainRegionService.HideProgressBar($"Файл сохранен: {saveExampleFilePath}");
        }

        private async void ClearDatabaseExecute()
        {
            mainRegionService.ShowProgressBar("Очистка базы данных.");

            var message = "Информация о пациентах будет удалена из базы данных. Продолжить ?";

            if (!await mainRegionService.ShowNotificationDialog(message))
            {
                mainRegionService.HideProgressBar("Очистка базы данных отменена.");
                return;
            }

            var db = new AppDBContext(settings.Common.SQLServer, settings.Common.SQLServerDB);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            PatientsCount = db.Patients.Count().ToString();
            mainRegionService.HideProgressBar("База данных очищена.");
        }
    }
}
