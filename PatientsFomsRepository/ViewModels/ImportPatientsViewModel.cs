using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : BindableBase, IViewModel
    {
        #region Поля
        private string progress;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }
        public string ImportFilePath { get; set; }
        public string SaveExampleFilePath { get; set; }
        public RelayCommandAsync ImportPatientsCommand { get; }
        public RelayCommandAsync SaveExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Загрузить в БД";
            FullCaption = "Загрузить известные ФИО из файла в базу данных";
            Progress = "";
            ImportPatientsCommand = new RelayCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new RelayCommandAsync(SaveExampleExecute);
        }
        #endregion

        #region Методы
        private void ImportPatientsExecute(object parameter)
        {
            if (string.IsNullOrEmpty(ImportFilePath))
                return;

            Progress = "Ожидайте. Открытие файла...";
            List<Patient> newPatients;
            using (var file = new ImportPatientsFile())
            {
                file.Open(ImportFilePath);
                newPatients = file.GetPatients();
                file.Dispose();
            }

            Progress = "Ожидайте. Проверка значений...";
            var db = new Models.Database();
            db.Patients.Load();
            var existenInsuaranceNumbers = db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();

            Progress = "Ожидайте. Сохранение в кэш...";
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();

            int total = existenInsuaranceNumbers.Count + newUniqPatients.Count;
            Progress = $"Завершено. В файле найдено {newPatients.Count} человек(а). В БД добавлено {newUniqPatients.Count}. Итого в БД {total}.";
        }
        private void SaveExampleExecute(object parameter)
        {
            if (string.IsNullOrEmpty(SaveExampleFilePath))
                return;

            Progress = "Ожидайте. Открытие файла...";
            ImportPatientsFile.SaveExample(SaveExampleFilePath);
            Progress = $"Завершено. Файл сохранен: {SaveExampleFilePath}";
        }
        #endregion
    }
}
