using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FomsPatientsDB.Models
    {
    public class Credential
        {        
        public string Login { get; set; }
        public string Password { get; set; }
        private int requestsLimit;
        public int RequestsLimit
            { get
                {
                return requestsLimit;
                }
            set
                {
                requestsLimit = value;
                requestsLeft = value;
                }
            }
        private int requestsLeft;
        private readonly object locker = new object();      

        public Credential Copy()
            {
            return MemberwiseClone() as Credential;
            }
        public bool TryReserveRequest()
            {
            lock(locker)
                {
                if (requestsLeft != 0)
                    {
                    requestsLeft--;
                    return true;
                    }
                else
                    return false;
                }
            }


        //возввращает учтеные данные по кругу пока лимит запросов не исчерпан
        public class RoundRobinCredentials
        {
            private readonly object locker = new object();
            private List<Credential> credentials;
            private int currentIndex;

            public RoundRobinCredentials(List<Credential> credentials)
            {
                this.credentials = new List<Credential>();
                credentials.ForEach(item => this.credentials.Add(item.Copy()));
                currentIndex = -1;
            }
            public bool TryGetNext(out Credential credential)
            {
                lock (locker)
                {
                    for (int i = 0; i < credentials.Count; i++)
                    {
                        MoveNext();

                        if (credentials[currentIndex].RequestsLimit > 0)
                        {
                            credential = credentials[currentIndex];
                            return true;
                        }
                    }

                    credential = null;
                    return false;
                }
            }
            private void MoveNext()
            {
                if (currentIndex + 1 < credentials.Count)
                    currentIndex++;
                else
                    currentIndex = 0;
            }
        }
    }
    }
