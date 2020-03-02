using CHI.Infrastructure;
using CHI.Models;
using CHI.Services.AttachedPatients;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.ViewModels
{
    public class AttachedPatientsStorageSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;
        private string patientsCount = "Вычисляется...";
        #endregion

        #region Свойства
        public string PatientsCount { get => patientsCount; set => SetProperty(ref patientsCount, value); }
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommandAsync ImportPatientsCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }
        public DelegateCommandAsync ClearDatabaseCommand { get; }
        #endregion

        #region Конструкторы
        public AttachedPatientsStorageSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;

            Task.Run(() => PatientsCount = new Models.AttachedPatientsDBContext().Patients.Count().ToString());

            ImportPatientsCommand = new DelegateCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new DelegateCommandAsync(ClearDatabaseExecute);
        }
        #endregion

        #region Методы
        private void ImportPatientsExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var importFilePath = fileDialogService.FileName;

            MainRegionService.SetBusyStatus("Открытие файла.");

            var newPatients = PatientsFileService.ReadImportPatientsFile(importFilePath);

            MainRegionService.SetBusyStatus("Проверка значений.");
            var db = new Models.AttachedPatientsDBContext();
            db.Patients.Load();

            var existenInsuaranceNumbers = new HashSet<string>(db.Patients.Select(x => x.InsuranceNumber));
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            MainRegionService.SetBusyStatus("Сохранение в локальную базу данных.");
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            PatientsCount = db.Patients.Count().ToString();

            MainRegionService.SetCompleteStatus($"В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count} новых человек(а).");
        }
        private void SaveExampleExecute()
        {
            fileDialogService.DialogType = FileDialogType.Save;
            fileDialogService.FileName = "Пример для загрузки ФИО";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FileName;

            MainRegionService.SetBusyStatus("Открытие файла.");
            PatientsFileService.SaveImportFileExample(saveExampleFilePath);
            MainRegionService.SetCompleteStatus($"Файл сохранен: {saveExampleFilePath}");
        }
        private void ClearDatabaseExecute()
        {
            MainRegionService.SetBusyStatus("Очистка базы данных.");

            var title = "Предупреждение";
            var message = "Информация о пациентах будет удалена из базы данных. Продолжить ?";
            var result = dialogService.ShowDialog(title, message);

            if (result == ButtonResult.Cancel)
            {
                MainRegionService.SetCompleteStatus("Очистка базы данных отменена.");
                return;
            }

            var db = new Models.AttachedPatientsDBContext();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            PatientsCount = db.Patients.Count().ToString();
            MainRegionService.SetCompleteStatus("База данных очищена.");
        }
        #endregion
    }
}
