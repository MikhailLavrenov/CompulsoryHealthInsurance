using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PatientsFomsRepository.ViewModels
{
    class PatientsFileViewModel : BindableBase, IViewModel
    {
        #region Поля
        private Settings settings;
        private DateTime fileDate;
        private string progress;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; } 
        public string FullCaption { get; set; }
        public string Progress { get => progress; set => SetProperty(ref progress, value); }         
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        public RelayCommand ProcessFileCommand { get; }
        #endregion

        #region Конструкторы
        public PatientsFileViewModel()
        {
            ShortCaption = "Получить ФИО";
            FullCaption = "Получить полные ФИО пациентов";
            Progress = "";
            Settings = Settings.Instance;
            FileDate = DateTime.Today;
            ProcessFileCommand = new RelayCommand(ProcessFileExecute, ProcessFileCanExecute);


        }
        #endregion

        #region Методы
        private async void ProcessFileExecute(object parameter)
        {
            Progress = "Проверка подключения к СРЗ";
            await Task.Run(() => Settings.TestConnection());

            if (Settings.DownloadNewPatientsFile)
            {
                if (Settings.ConnectionIsValid == false)
                {
                    Progress = "Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта. Без подключения к СРЗ возможно только подставить ФИО из кэша в существующий файл.";
                    return;
                }

                Progress = "Загрузка файла из СРЗ";
                await Task.Run(() =>
                {
                    SRZ site;
                    if (Settings.UseProxy)
                        site = new SRZ(Settings.SiteAddress, Settings.ProxyAddress, Settings.ProxyPort);
                    else
                        site = new SRZ(Settings.SiteAddress);

                    var credential = Settings.Credentials.First(x => x.RequestsLimit > 0);
                    site.TryAuthorize(credential);
                    site.GetPatientsFile(Settings.PatientsFilePath, FileDate);
                });
            }

            Progress = "Подстановка ФИО из кэша";
            var db = new Models.Database();
            var file = new PatientsFile();

            await Task.Run(() =>
            {
                db.Patients.Load();
                file.Open(Settings.PatientsFilePath, Settings.ColumnProperties);
                file.SetFullNames(db.Patients.ToList());
            });

            string resultReport;

            if (Settings.ConnectionIsValid)
            {
                Progress = "Поиск пациентов без ФИО в файле";
                long limitCount = 0;
                var unknownInsuaranceNumbers = new List<string>();

                await Task.Run(() =>
                {
                    limitCount = Settings.Credentials.Sum(x => x.RequestsLimit);
                    unknownInsuaranceNumbers = file.GetUnknownInsuaranceNumbers(limitCount);
                });

                Progress = "Поиск ФИО в СРЗ";
                var verifiedPatients = new Patient[0];

                await Task.Run(() =>
                verifiedPatients = GetPatients(unknownInsuaranceNumbers));

                Progress = "Подстановка в файл ФИО найденных в СРЗ";
                await Task.Run(() => file.SetFullNames(verifiedPatients));

                Progress = "Добавление в кэш ФИО найденных в СРЗ";
                await Task.Run(() =>
                {
                    var duplicateInsuranceNumber = verifiedPatients.Select(x => x.InsuranceNumber).ToHashSet();
                    var duplicatePatients = db.Patients.Where(x => duplicateInsuranceNumber.Contains(x.InsuranceNumber)).ToArray();
                    db.Patients.RemoveRange(duplicatePatients);
                    db.SaveChanges();

                    db.Patients.AddRange(verifiedPatients);
                    db.SaveChanges();
                });

                resultReport = $"Завершено. В СРЗ найдено {verifiedPatients.Count()} человек, лимит {limitCount}. ";
            }
            else
                resultReport = $"Завершено. ФИО подставлены только из кэша.  Не удалось подключиться к СРЗ, проверьте настройки и работоспособность сайта.";

            Progress = "Подсчет человек без ФИО";
            var unknownPatients = new List<string>();
            await Task.Run(() => unknownPatients = file.GetUnknownInsuaranceNumbers(int.MaxValue));


            if (unknownPatients.Count == 0)
            {
                Progress = "Форматирование файла";
                await Task.Run(() => file.Format());
            }

            Progress = "Сохранение изменений";
            await Task.Run(() => file.Save());

            Progress = $"{ resultReport} Осталось найти {unknownPatients.Count} ФИО.";
        }
        private bool ProcessFileCanExecute(object parameter)
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
