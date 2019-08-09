using OfficeOpenXml;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            ShortCaption = "Загрузить полные ФИО";
            FullCaption = "Загрузить полные ФИО в кэш";
            Progress = "";
            ImportCommand = new RelayCommand(ImportExecute, ImportCanExecute);
            SaveExampleCommand = new RelayCommand(x => ImportPatientsFile.SaveExample((string)x));
        }
        #endregion

        #region Методы
        private async void ImportExecute(object parameter)
        {
            Progress = "Ожидайте. Открытие файла...";
            await Task.Run(() =>
            {
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
            });
            Progress = "Завершено.";
        }
        private bool ImportCanExecute(object parameter)
        {
            string filePath = parameter as string;
            return File.Exists(filePath);
        }
        #endregion
    }
}
