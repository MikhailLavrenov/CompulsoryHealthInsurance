using CHI.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.Services.SRZ
{
    public class ParallelSRZService : ParallelWebServiceBase
    {
        public ParallelSRZService(string address, ICredential credential, int maxDegreeOfParallelism)
            : base(address, credential, maxDegreeOfParallelism)
        {
        }


        public async Task<List<Patient>> GetPatientsAsync(IEnumerable<string> insuranceNumbers)
        {
            var verifiedPatients = new ConcurrentBag<Patient>();
            var enpStack = new ConcurrentStack<string>(insuranceNumbers);
            var tasks = new Task[Math.Min(maxDegreeOfParallelism, insuranceNumbers.Count())];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var service = new SRZService(address, useProxy, proxyAddress, proxyPort);

                    await service.AuthorizeAsync(credential);

                    while (enpStack.TryPop(out string enp))
                    {
                        var patient = await service.GetPatientAsync(enp);

                        if (patient != null)
                        {
                            verifiedPatients.Add(patient);
                            OnProgressChanged(verifiedPatients.Count);
                        }
                    }

                    await service.LogoutAsync();
                });
            }

            await Task.WhenAll(tasks);

            return verifiedPatients.ToList();
        }
    }
}
