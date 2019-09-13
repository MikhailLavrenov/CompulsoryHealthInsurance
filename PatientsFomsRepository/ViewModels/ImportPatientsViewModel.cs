using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Regions;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace PatientsFomsRepository.ViewModels
{
    class ImportPatientsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        #endregion

        #region Свойства
        public IActiveViewModel ActiveViewModel { get; set; }
        public bool KeepAlive { get => false; }
        public string ImportFilePath { get; set; }
        public string SaveExampleFilePath { get; set; }
        public DelegateCommandAsync ImportPatientsCommand { get; }
        public DelegateCommandAsync SaveExampleCommand { get; }
        public DelegateCommandAsync ClearDatabaseCommand { get; }
        #endregion

        #region Конструкторы
        public ImportPatientsViewModel()
        {
        }
        public ImportPatientsViewModel(IActiveViewModel activeViewModel)
        {            
            ActiveViewModel = activeViewModel;

            ActiveViewModel.Header = "Загрузить известные ФИО из файла в базу данных";
            ImportPatientsCommand = new DelegateCommandAsync(ImportPatientsExecute);
            SaveExampleCommand = new DelegateCommandAsync(SaveExampleExecute);
            ClearDatabaseCommand = new DelegateCommandAsync(ClearDatabaseExecute);
        }
        #endregion

        #region Методы
        private void ImportPatientsExecute()
        {
            if (string.IsNullOrEmpty(ImportFilePath))
                return;

            ActiveViewModel.Status = "Ожидайте. Открытие файла...";
            List<Patient> newPatients;
            using (var file = new ImportPatientsFile())
            {
                file.Open(ImportFilePath);
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
            if (string.IsNullOrEmpty(SaveExampleFilePath))
                return;

            ActiveViewModel.Status = "Ожидайте. Открытие файла...";
            ImportPatientsFile.SaveExample(SaveExampleFilePath);
            ActiveViewModel.Status = $"Завершено. Файл сохранен: {SaveExampleFilePath}";
        }
        private void ClearDatabaseExecute()
        {
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
