using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.ViewModels
{
    class PatientsFileViewModel : BindableBase, IViewModel
    {
        #region Поля
        private Settings settings;
        private DateTime fileDate;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
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
            ProcessFileCommand = new RelayCommand(ProcessFileExecute, ProcessFileCanEcecute);


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
            SRZ site;
            if (Settings.UseProxy)
             site = new SRZ(Settings.SiteAddress, Settings.ProxyAddress, Settings.ProxyPort);
            else 
                site= new SRZ(Settings.SiteAddress);

            if (Settings.DownloadNewPatientsFile)
            {
                site.TryAuthorize(Settings.Credentials.First(x => x.RequestsLimit > 0));
                await site.GetPatientsFile(Settings.PatientsFilePath, FileDate);
            }

            var db = new Models.Database();
            db.Patients.Load();
            var file = new PatientsFile();
            await file.Open(@"C:\Users\ЛавреновМВ\Desktop\attmo.xlsx");
            await file.SetFullNames(db.Patients);
            var limitCount = Settings.Credentials.Sum(x => x.RequestsLimit);
            var unverifiedInsuaranceNumbers = await file.GetUnverifiedInsuaranceNumbersAsync(limitCount);
            var verifiedPatients = GetPatients(unverifiedInsuaranceNumbers);
            db.Patients.AddRange(verifiedPatients);
            db.SaveChanges();
            await file.SetFullNames(db.Patients);                   
        }
        private bool ProcessFileCanEcecute (object parameter)
        {
            if (Settings.DownloadNewPatientsFile == false && File.Exists(Settings.PatientsFilePath) == false)
                return false;
            else
                return true;
        }

        #endregion
    }
}
