using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private IDialogService dialogService;
        private readonly IFileDialogService fileDialogService;
        #endregion

        #region Свойства
        public IActiveViewModel ActiveViewModel { get; set; }
        public bool KeepAlive { get => false; }
        public DelegateCommandAsync ImportPatientsCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }
        public DelegateCommandAsync ClearDatabaseCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel(IActiveViewModel activeViewModel, IFileDialogService fileDialogService, IDialogService dialogService)
        {
            ActiveViewModel = activeViewModel;
            this.fileDialogService = fileDialogService;
            this.dialogService = dialogService;

            ActiveViewModel.Header = "Загрузить известные ФИО из файла в базу данных";
            ImportPatientsCommand = new DelegateCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new DelegateCommandAsync(ClearDatabaseExecute);
        }
        #endregion

        #region Методы
        private void ImportPatientsExecute()
        {
            fileDialogService.DialogType = DialogType.Open;
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var importFilePath = fileDialogService.FullPath;

            ActiveViewModel.Status = "Ожидайте. Открытие файла...";
            List<Patient> newPatients;
            using (var file = new ImportPatientsFile())
            {
                file.Open(importFilePath);
                newPatients = file.GetPatients();
                file.Dispose();
            }

            ActiveViewModel.Status = "Ожидайте. Проверка значений...";
            var db = new Models.Database();
            db.Patients.Load();
            var existenInsuaranceNumbers = db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            ActiveViewModel.Status = "Ожидайте. Сохранение в кэш...";
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            int total = existenInsuaranceNumbers.Count + newUniqPatients.Count;
            ActiveViewModel.Status = $"Завершено. В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count} новых. Итого в БД {total}.";
        }
        private void SaveExampleExecute()
        {
            fileDialogService.DialogType = DialogType.Save;
            fileDialogService.FullPath = "Пример для загрузки ФИО";
            fileDialogService.Filter = "Excel files (*.xslx)|*.xlsx";

            if (fileDialogService.ShowDialog() != true)
                return;

            var saveExampleFilePath = fileDialogService.FullPath;

            ActiveViewModel.Status = "Ожидайте. Открытие файла...";
            ImportPatientsFile.SaveExample(saveExampleFilePath);
            ActiveViewModel.Status = $"Завершено. Файл сохранен: {saveExampleFilePath}";
        }
        private void ClearDatabaseExecute()
        {
            var message = "Очистка базы данных приведет к потере всех сохраненных данных пациентов. Продолжить ?";
            var result = dialogService.ShowDialog(message);

            if (result == ButtonResult.Cancel)
                return;

            ActiveViewModel.Status = "Ожидайте. Очистка базы данных...";
            var db = new Models.Database();
            if (db.Database.Exists())
                db.Database.Delete();
            db.Database.Create();
            ActiveViewModel.Status = "Завершено. База данных очищена.";
        }
        #endregion
    }
}
