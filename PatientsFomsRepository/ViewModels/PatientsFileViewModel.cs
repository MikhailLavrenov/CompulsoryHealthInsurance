using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PatientsFomsRepository.ViewModels
{
    class PatientsFileViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private DateTime fileDate;
        #endregion

        #region Свойства
        public IActiveViewModel ActiveViewModel { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        public DelegateCommandAsync ProcessFileCommand { get; }
        #endregion

        #region Конструкторы
        public PatientsFileViewModel()
        {
        }
        public PatientsFileViewModel(IActiveViewModel activeViewModel)
        {
            ActiveViewModel = activeViewModel;
            Settings = Settings.Instance;

            ActiveViewModel.Header = "Получить полные ФИО пациентов";            
            FileDate = DateTime.Today;            
            ProcessFileCommand = new DelegateCommandAsync(ProcessFileExecute, ProcessFileCanExecute);
        }
        #endregion

        #region Методы
        private void ProcessFileExecute()
        {
            ActiveViewModel.Status = "Ожидайте. Проверка подключения к СРЗ...";
            Settings.TestConnection();

            if (Settings.DownloadNewPatientsFile)
            {
                if (Settings.ConnectionIsValid == false)
                {
                    ActiveViewModel.Status = "Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта. Без подключения к СРЗ возможно только подставить ФИО из кэша в существующий файл.";
                    return;
                }

                ActiveViewModel.Status = "Ожидайте. Загрузка файла из СРЗ...";

                SRZ site;
                if (Settings.UseProxy)
                    site = new SRZ(Settings.SiteAddress, Settings.ProxyAddress, Settings.ProxyPort);
                else
                    site = new SRZ(Settings.SiteAddress);

                var credential = Settings.Credentials.First(x => x.RequestsLimit > 0);
                site.TryAuthorize(credential);
                site.GetPatientsFile(Settings.PatientsFilePath, FileDate);
            }

            ActiveViewModel.Status = "Ожидайте. Подстановка ФИО из кэша...";
            var db = new Models.Database();
            var file = new PatientsFile();

            db.Patients.Load();
            file.Open(Settings.PatientsFilePath, Settings.ColumnProperties);
            file.SetFullNames(db.Patients.ToList());

            string resultReport;

            if (Settings.ConnectionIsValid)
            {
                ActiveViewModel.Status = "Ожидайте. Поиск пациентов без ФИО в файле...";
                var limitCount = Settings.Credentials.Sum(x => x.RequestsLimit);
                var unknownInsuaranceNumbers = file.GetUnknownInsuaranceNumbers(limitCount);

                ActiveViewModel.Status = "Ожидайте. Поиск ФИО в СРЗ...";
                var verifiedPatients = GetPatients(unknownInsuaranceNumbers);

                ActiveViewModel.Status = "Ожидайте. Подстановка в файл ФИО найденных в СРЗ...";
                file.SetFullNames(verifiedPatients);

                ActiveViewModel.Status = "Ожидайте. Добавление в кэш ФИО найденных в СРЗ...";
                var duplicateInsuranceNumber = verifiedPatients.Select(x => x.InsuranceNumber).ToHashSet();
                var duplicatePatients = db.Patients.Where(x => duplicateInsuranceNumber.Contains(x.InsuranceNumber)).ToArray();
                db.Patients.RemoveRange(duplicatePatients);
                db.SaveChanges();

                db.Patients.AddRange(verifiedPatients);
                db.SaveChanges();

                resultReport = $"Завершено. В СРЗ запрошено {verifiedPatients.Count()} человек, лимит {limitCount}. ";
            }
            else
                resultReport = $"Завершено. ФИО подставлены только из кэша.  Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта.";

            ActiveViewModel.Status = "Ожидайте. Подсчет человек без ФИО...";
            var unknownPatients = file.GetUnknownInsuaranceNumbers(int.MaxValue);

            if (unknownPatients.Count == 0)
            {
                ActiveViewModel.Status = "Ожидайте. Форматирование файла...";
                file.Format();
            }

            ActiveViewModel.Status = "Ожидайте. Сохранение изменений...";
            file.Save();

            ActiveViewModel.Status = $"{ resultReport} Осталось найти {unknownPatients.Count} ФИО.";
        }
        private bool ProcessFileCanExecute()
        {
            if (Settings.DownloadNewPatientsFile == false && File.Exists(Settings.PatientsFilePath) == false)
                return false;
            else
                return true;
        }
        //запускает многопоточно запросы к сайту для поиска пациентов
        private Patient[] GetPatients(List<string> insuranceNumbers)
        {
            int threadsLimit = Settings.ThreadsLimit;
            if (insuranceNumbers.Count < threadsLimit)
                threadsLimit = insuranceNumbers.Count;

            var robinRoundCredentials = new RoundRobinCredentials(Settings.Credentials);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<SRZ>[threadsLimit];
            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => { return (SRZ)null; });

            for (int i = 0; i < insuranceNumbers.Count; i++)
            {
                var insuranceNumber = insuranceNumbers[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var site = task.Result;
                    if (site == null || site.Credential.TryReserveRequest() == false)
                    {
                        if (site != null)
                            site.Logout();

                        while (true)
                        {
                            if (robinRoundCredentials.TryGetNext(out Credential credential) == false)
                                return null;

                            if (credential.TryReserveRequest())
                            {
                                if (Settings.UseProxy)
                                    site = new SRZ(Settings.SiteAddress, Settings.ProxyAddress, Settings.ProxyPort);
                                else
                                    site = new SRZ(Settings.SiteAddress);

                                if (site.TryAuthorize(credential))
                                    break;
                            }
                        }
                    }

                    if (site.TryGetPatient(insuranceNumber, out Patient patient))
                        verifiedPatients.Add(patient);

                    return site;
                });
            }
            Task.WaitAll(tasks);

            return verifiedPatients.ToArray();
        }
        #endregion
    }
}
