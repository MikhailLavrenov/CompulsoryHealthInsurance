using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : BindableBase, IViewModel
    {
        #region Поля
        #endregion

        #region Свойства
        public IStatusBar StatusBar { get; set; }
        public bool KeepAlive { get => false; }
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public string ImportFilePath { get; set; }
        public string SaveExampleFilePath { get; set; }
        public RelayCommandAsync ImportPatientsCommand { get; }
        public RelayCommandAsync SaveExampleCommand { get; }
        public RelayCommandAsync ClearDatabaseCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
        }
        public ImportPatientsViewModel(IStatusBar statusBar)
        {
            ShortCaption = "Загрузить в БД";
            FullCaption = "Загрузить известные ФИО из файла в базу данных";
            StatusBar = statusBar;
            ImportPatientsCommand = new RelayCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new RelayCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new RelayCommandAsync(ClearDatabaseExecute);
        }
        #endregion

        #region Методы
        private void ImportPatientsExecute(object parameter)
        {
            if (string.IsNullOrEmpty(ImportFilePath))
                return;

            StatusBar.StatusText = "Ожидайте. Открытие файла...";
            List<Patient> newPatients;
            using (var file = new ImportPatientsFile())
            {
                file.Open(ImportFilePath);
                newPatients = file.GetPatients();
                file.Dispose();
            }

            StatusBar.StatusText = "Ожидайте. Проверка значений...";
            var db = new Models.Database();
            db.Patients.Load();
            var existenInsuaranceNumbers = db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            StatusBar.StatusText = "Ожидайте. Сохранение в кэш...";
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            int total = existenInsuaranceNumbers.Count + newUniqPatients.Count;
            StatusBar.StatusText = $"Завершено. В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count} новых. Итого в БД {total}.";
        }
        private void SaveExampleExecute(object parameter)
        {
            if (string.IsNullOrEmpty(SaveExampleFilePath))
                return;

            StatusBar.StatusText = "Ожидайте. Открытие файла...";
            ImportPatientsFile.SaveExample(SaveExampleFilePath);
            StatusBar.StatusText = $"Завершено. Файл сохранен: {SaveExampleFilePath}";
        }
        private void ClearDatabaseExecute(object parameter)
        {
            StatusBar.StatusText = "Ожидайте. Очистка базы данных...";
            var db = new Models.Database();
            if (db.Database.Exists())
                db.Database.Delete();
            db.Database.Create();
            StatusBar.StatusText = "Завершено. База данных очищена.";
        }
        #endregion
    }
}
