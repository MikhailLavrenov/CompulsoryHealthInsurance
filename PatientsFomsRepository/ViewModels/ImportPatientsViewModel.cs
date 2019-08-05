using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System;
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
        public RelayCommand GetExampleCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
            ShortCaption = "Пополнить БД";
            FullCaption = "Пополнить БД известными ФИО из файла";
            ImportCommand = new RelayCommand(ImportExecute, ImportCanExecute);
            GetExampleCommand = new RelayCommand(GetExampleExecute);

        }
        #endregion

        #region Методы
        private void GetExampleExecute(object parameter)
        {
            string destinationFile = parameter as string;



            throw new NotImplementedException("еще не реализовано");
        }
        private async void ImportExecute(object parameter)
        {
            string filePath = (string)parameter;
            var columnsProperty = Settings.Instance.ColumnsProperty.ToArray();

            var patientsFile = new PatientsFile();
            await patientsFile.Open(filePath, columnsProperty);
            var newPatients = await patientsFile.GetVerifedPatientsAsync();

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
