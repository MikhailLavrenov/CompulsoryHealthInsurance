using CHI.Application.Infrastructure;
using CHI.Application.Models;
using CHI.Services.AttachedPatients;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace CHI.Application.ViewModels
{
    public class OtherSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommand ShowFileDialogCommand { get; }
        public DelegateCommandAsync ImportPatientsCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }
        public DelegateCommandAsync ClearDatabaseCommand { get; }
        #endregion

        #region Конструкторы
        public OtherSettingsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;
            MainRegionService = mainRegionService;

            Settings = Settings.Instance;
            MainRegionService.Header = "Прочие настройки";

            SaveCommand = new DelegateCommand(SaveExecute);
            LoadCommand = new DelegateCommand(LoadExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            ShowFileDialogCommand = new DelegateCommand(ShowFileDialogExecute);
            ImportPatientsCommand = new DelegateCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new DelegateCommandAsync(ClearDatabaseExecute);
        }
        #endregion

        #region Методы
        private void ShowFileDialogExecute()
        {
            fileDialogService.DialogType = FileDialogType.Open;
            fileDialogService.FileName = settings.PatientsFilePath;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() == true)
                settings.PatientsFilePath = fileDialogService.FileName;
        }
        private void SaveExecute()
        {
            Settings.Save();
            MainRegionService.SetCompleteStatus("Настройки сохранены.");
        }
        private void LoadExecute()
        {
            Settings = Settings.Load();
            MainRegionService.SetCompleteStatus("Изменения настроек отменены.");
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultMedicalExaminations();
            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
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
            var db = new Models.Database();
            db.Patients.Load();

            var existenInsuaranceNumbers = new HashSet<string>(db.Patients.Select(x => x.InsuranceNumber));
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            MainRegionService.SetBusyStatus("Сохранение в кэш.");
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            int total = existenInsuaranceNumbers.Count + newUniqPatients.Count;
            MainRegionService.SetCompleteStatus($"В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count} новых. Итого в БД {total}.");
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
            var title = "Предупреждение";
            var message = "Информация о пациентах будет удалена из базы данных. Продолжить ?";
            var result = dialogService.ShowDialog(title, message);

            if (result == ButtonResult.Cancel)
            {
                MainRegionService.SetCompleteStatus("Очистка базы данных отменена.");
                return;
            }

            MainRegionService.SetBusyStatus("Очистка базы данных.");
            var db = new Models.Database();
            if (db.Database.Exists())
                db.Database.Delete();
            db.Database.Create();
            MainRegionService.SetCompleteStatus("База данных очищена.");
        }
        #endregion
    }
}
