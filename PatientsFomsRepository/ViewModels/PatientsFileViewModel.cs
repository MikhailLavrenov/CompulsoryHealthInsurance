using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Concurrent;
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
        public RelayCommand ProcessFileCommand { get; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public DateTime FileDate { get => fileDate; set => SetProperty(ref fileDate, value); }
        #endregion

        #region Конструкторы
        public PatientsFileViewModel()
        {
            ShortCaption = "Получить полные ФИО";
            FullCaption = "Получить полные ФИО пациентов";
            Settings = Settings.Instance;
            FileDate = DateTime.Today;
            ProcessFileCommand = new RelayCommand(ProcessFileExecute, ProcessFileCanExecute);
            Progress = "";

        }
        #endregion

        #region Методы
        //запускает многопоточно запросы к сайту для поиска пациентов
        private Patient[] GetPatients(string[] insuranceNumbers)
        {
            int threadsLimit = Settings.ThreadsLimit;
            if (insuranceNumbers.Length < threadsLimit)
                threadsLimit = insuranceNumbers.Length;

            var robinRoundCredentials = new RoundRobinCredentials(Settings.Credentials);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<SRZ>[threadsLimit];
            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => { return (SRZ)null; });

            for (int i = 0; i < insuranceNumbers.Length; i++)
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
        private async void ProcessFileExecute(object parameter)
        {
            if (Settings.DownloadNewPatientsFile)
            {
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
                file.Open(Settings.PatientsFilePath);
                file.SetFullNames(db.Patients.ToList());
            });

            Progress = "Поиск пациентов без ФИО в файле";
            var limitCount = 0;
            var unverifiedInsuaranceNumbers = new string[0];

            await Task.Run(() =>
            {
                limitCount = Settings.Credentials.Sum(x => x.RequestsLimit);
                unverifiedInsuaranceNumbers = file.GetUnverifiedInsuaranceNumbersAsync(limitCount);
            });

            Progress = "Поиск ФИО в СРЗ";
            var verifiedPatients = new Patient[0];

            await Task.Run(() =>
            verifiedPatients = GetPatients(unverifiedInsuaranceNumbers));

            Progress = "Подстановка в файл ФИО найденных в СРЗ";
            await Task.Run(() =>
            {
                file.SetFullNames(verifiedPatients);
                file.Save();
            });

            Progress = "Добавление в кэш ФИО найденных в СРЗ";
            await Task.Run(() =>
            {
                db.Patients.AddRange(verifiedPatients);
                db.SaveChanges();
            });

            Progress = $"Завершено, из СРЗ загружено {verifiedPatients.Count()} из разрешенных {limitCount}";
        }
        private bool ProcessFileCanExecute(object parameter)
        {
            if (Settings.DownloadNewPatientsFile == false && File.Exists(Settings.PatientsFilePath) == false)
                return false;
            else
                return true;
        }

        #endregion
    }
}
