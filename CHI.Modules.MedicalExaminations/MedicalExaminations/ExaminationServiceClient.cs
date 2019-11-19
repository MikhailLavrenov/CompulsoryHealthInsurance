using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationServiceClient
    {
        public IEnumerable<Credential> Credentials { get; set; }
        public string URL { get; set; }
        public string ProxyAdress { get; set; }
        public int ProxyPort { get; set; }
        public int ThreadsLimit { get; set; }
        public bool UseProxy { get; set; }

        public void AddPatiensExaminations(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = ThreadsLimit;

            if (patientsExaminations.Count < threadsLimit)
                threadsLimit = patientsExaminations.Count;

            var circularList = new CircularList<Credential>(Credentials);
            var verifiedPatients = new ConcurrentBag<PatientExaminations>();
            var tasks = new Task<ExaminationService>[threadsLimit];

            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => { return (ExaminationService)null; });

            for (int i = 0; i < patientsExaminations.Count; i++)
            {
                var patientExaminations = patientsExaminations[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var service = task.Result;
                    if (service == null)
                    {
                        if (UseProxy)
                            service = new ExaminationService(URL, ProxyAdress, ProxyPort);
                        else
                            service = new ExaminationService(URL);

                        service.Authorize(circularList.GetNext());
                    }

                    service.AddPatientExaminations(patientExaminations);


                    //if (service.TryGetPatient(patientExaminations, out Patient patient))
                    //    verifiedPatients.Add(patient);

                    return service;

                });
            }
            Task.WaitAll(tasks);

            //return verifiedPatients.ToArray();
        }
    }
}
