using CHI.Services.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationServiceParallel
    {
        public IEnumerable<ICredential> Credentials { get; private set; }
        public string URL { get; private set; }
        public string ProxyAddress { get; private set; }
        public int ProxyPort { get; private set; }
        public int ThreadsLimit { get; private set; }
        public bool UseProxy { get; private set; }

        public ExaminationServiceParallel(string url, int threadsLimit, IEnumerable<ICredential> credentials)
            :this(url,null,0, threadsLimit, credentials)
        {
        }
        public ExaminationServiceParallel(string url, string proxyAddress, int proxyPort, int threadsLimit, IEnumerable<ICredential> credentials)
        {
            URL = url;
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;
            ThreadsLimit = threadsLimit;
            Credentials = credentials;
        }

        public List<PatientExaminations> AddPatientsExaminations(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = ThreadsLimit;

            if (patientsExaminations.Count < threadsLimit)
                threadsLimit = patientsExaminations.Count;

            var circularList = new CircularList<ICredential>(Credentials);
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
                    var service = task.ConfigureAwait(false).GetAwaiter().GetResult();
                    if (service == null)
                    {
                        if (UseProxy)
                            service = new ExaminationService(URL, ProxyAddress, ProxyPort);
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
