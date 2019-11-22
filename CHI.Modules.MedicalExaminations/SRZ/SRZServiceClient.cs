using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.SRZ
{
    public class SRZServiceClient
    {
        public IEnumerable<Credential> Credentials { get; private set; }
        public string URL { get; private set; }
        public string ProxyAddress { get; private set; }
        public int ProxyPort { get; private set; }
        public int ThreadsLimit { get; private set; }
        public bool UseProxy { get; private set; }

        public SRZServiceClient(string url, int threadsLimit, IEnumerable<Credential> credentials)
            : this(url, null, 0, threadsLimit, credentials)
        {
        }
        public SRZServiceClient(string url, string proxyAddress, int proxyPort, int threadsLimit, IEnumerable<ICredential> credentials)
        {
            URL = url;
            ProxyAddress = proxyAddress;
            ProxyPort = proxyPort;
            ThreadsLimit = threadsLimit;
            Credentials = credentials;
        }

        private Patient[] GetPatients(List<string> insuranceNumbers)
        {
            int threadsLimit = ThreadsLimit;
            if (insuranceNumbers.Count < threadsLimit)
                threadsLimit = insuranceNumbers.Count;

            var robinRoundCredentials = new CircularCredentials(Credentials);
            var verifiedPatients = new ConcurrentBag<Patient>();
            var tasks = new Task<SRZService>[threadsLimit];
            for (int i = 0; i < threadsLimit; i++)
                tasks[i] = Task.Run(() => { return (SRZService)null; });

            for (int i = 0; i < insuranceNumbers.Count; i++)
            {
                var insuranceNumber = insuranceNumbers[i];
                var index = Task.WaitAny(tasks);
                tasks[index] = tasks[index].ContinueWith((task) =>
                {
                    var site = task.ConfigureAwait(false).GetAwaiter().GetResult();
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
                                if (UseProxy)
                                    site = new SRZService(URL, ProxyAddress, ProxyPort);
                                else
                                    site = new SRZService(URL);

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
    }
}
