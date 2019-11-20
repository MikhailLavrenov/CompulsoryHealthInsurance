using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationServiceClient
    {
        public IEnumerable<SimpleCredential> Credentials { get; private set; }
        public string URL { get; private set; }
        public string ProxyAdress { get; private set; }
        public int ProxyPort { get; private set; }
        public int ThreadsLimit { get; private set; }
        public bool UseProxy { get; private set; }

        public ExaminationServiceClient(string url, int threadsLimit, IEnumerable<SimpleCredential> credentials)
            :this(url,null,0, threadsLimit, credentials)
        {
        }
        public ExaminationServiceClient(string url, string proxyAdress, int proxyPort, int threadsLimit, IEnumerable<SimpleCredential> credentials)
        {
            URL = url;
            ProxyAdress = proxyAdress;
            ProxyPort = proxyPort;
            ThreadsLimit = threadsLimit;
            Credentials = credentials;
        }

        public List<PatientExaminations> AddPatientsExaminations(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = ThreadsLimit;

            if (patientsExaminations.Count < threadsLimit)
                threadsLimit = patientsExaminations.Count;

            var circularList = new CircularList<SimpleCredential>(Credentials);
            var errors = new ConcurrentBag<PatientExaminations>();
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

                    if(!service.TryAddPatientExaminations(patientExaminations))
                        errors.Add(patientExaminations);

                    return service;
                });
            }
            Task.WaitAll(tasks);

            return errors.ToList();
        }
    }
}
