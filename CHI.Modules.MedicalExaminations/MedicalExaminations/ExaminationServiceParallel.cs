using CHI.Services.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public event EventHandler<CounterEventArgs> AddCounterChangeEvent;

        public ExaminationServiceParallel(string url, int threadsLimit, IEnumerable<ICredential> credentials)
            : this(url, false, null, 0, threadsLimit, credentials)
        {
        }
        public ExaminationServiceParallel(string url, bool useProxy, string proxyAddress, int proxyPort, int threadsLimit, IEnumerable<ICredential> credentials)
        {
            URL = url;
            UseProxy = useProxy;
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;
            ThreadsLimit = threadsLimit;
            Credentials = credentials;
        }

        public List<Tuple<PatientExaminations, bool, string>> AddPatientsExaminations(List<PatientExaminations> patientsExaminations)
        {
            var threadsLimit = ThreadsLimit;

            if (patientsExaminations.Count < threadsLimit)
                threadsLimit = patientsExaminations.Count;

            var circularList = new CircularList<ICredential>(Credentials);
            var result = new ConcurrentBag<Tuple<PatientExaminations, bool, string>>();
            var tasks = new Task<ExaminationService>[threadsLimit];
            var counter = 0;

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

                    var error = string.Empty;
                    var isSuccessful = true;

                    try
                    {
                        service.AddPatientExaminations(patientExaminations);                      
                    }
                    catch (InvalidOperationException ex)
                    {
                        error = ex.Message;
                        isSuccessful = false;
                    }
                    catch (WebServiceOperationException ex)
                    {
                        error = ex.Message;
                        isSuccessful = false;
                    }

                    result.Add(new Tuple<PatientExaminations,bool, string>(patientExaminations, isSuccessful, error));
                    Interlocked.Increment(ref counter);
                    AddCounterChangeEvent(null, new CounterEventArgs(counter, patientsExaminations.Count));

                    return service;
                });
            }
            Task.WaitAll(tasks);

            return result.ToList();
        }
    }
}
