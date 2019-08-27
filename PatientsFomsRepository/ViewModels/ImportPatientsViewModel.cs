using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Data.Entity;
using System.IO;
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
        public RelayCommand ImportCommand { get; }
        public RelayCommand SaveExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Загрузить в БД";
            FullCaption = "Загрузить известные ФИО из файла в базу данных";
            Progress = "";
            ImportCommand = new RelayCommand(ImportExecute, ImportCanExecute);
            SaveExampleCommand = new RelayCommand(SaveExampleExecute);
        }
        #endregion

        #region Методы
        private void ImportExecute(object parameter)
        {
            Progress = "Ожидайте. Открытие файла...";
            string filePath = (string)parameter;
            var file = new ImportPatientsFile();
            file.Open(filePath);
            var newPatients = file.GetPatients();
            file.Dispose();

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
        private bool ImportCanExecute(object parameter)
        {
            string filePath = parameter as string;
            return File.Exists(filePath);
        }
        private void SaveExampleExecute(object parameter)
        {
            var path = (string)parameter;
            ImportPatientsFile.SaveExample(path);
            Progress = $"Завершено. Файл сохранен: {path}";
        }
        #endregion
    }
}
