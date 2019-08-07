using OfficeOpenXml;
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
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand SaveExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Загрузить полные ФИО";
            FullCaption = "Загрузить полные ФИО в кэш";
            ImportCommand = new RelayCommand(ImportExecute, ImportCanExecute);
            SaveExampleCommand = new RelayCommand(x => ImportPatientsFile.SaveExample((string)x));
        }
        #endregion

        #region Методы
        private async void ImportExecute(object parameter)
        {
            string filePath = (string)parameter;
            var file = new ImportPatientsFile();
            await file.OpenAsync(filePath);
            var newPatients = await file.GetPatientsAsync();
            file.Dispose();

            var db = new Models.Database();
            db.Patients.Load();
            var existenInsuaranceNumbers = db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x => !existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x => x.InsuranceNumber)
            .Select(x => x.First())
            .ToList();
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();
        }
        private bool ImportCanExecute(object parameter)
        {
            string filePath = parameter as string;
            return File.Exists(filePath);
        }
        #endregion
    }
}
