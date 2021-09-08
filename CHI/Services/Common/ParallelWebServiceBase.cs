using CHI.Models;
using System;

namespace CHI.Services
{
    public abstract class ParallelWebServiceBase
    {
        protected string address;
        protected bool useProxy;
        protected string proxyAddress;
        protected ushort proxyPort;
        protected ICredential credential;
        protected int maxDegreeOfParallelism;
        public delegate void ProgressHandler (int proceedCount);
        public event ProgressHandler ProgressChanged;


        public ParallelWebServiceBase(string address, ICredential credential, int maxDegreeOfParallelism)
        {
            this.address = address;
            this.credential = credential;
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public void UseProxy(string proxyAddress, ushort proxyPort)
        {
            useProxy = true;
            this.proxyAddress = proxyAddress;
            this.proxyPort = proxyPort;
        }

        protected void OnProgressChanged(int proceedCount)
            => ProgressChanged?.Invoke(proceedCount);
    }
}
