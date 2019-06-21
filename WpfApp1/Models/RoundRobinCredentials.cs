using System.Collections.Generic;
using System.Linq;

namespace FomsPatientsDB.Models
    {

    //возввращает учтеные данные по кругу пока лимит запросов не исчерпан
    class RoundRobinCredentials
        {
        private readonly object syncLock = new object();
        private List<Credential> credentials;
        private int currentIndex;
        private int count;

        public RoundRobinCredentials(List<Credential> credentials)
            {
            this.credentials = new List<Credential>();
            credentials.ForEach(x => this.credentials.Add(x.Copy()));
            currentIndex = -1;
            count = credentials.Count;
            }

        public bool TryGetNext(out Credential credential)
            {
            lock (syncLock)
                {
                MoveNext();

                int iterator = 0;
                while (credentials[currentIndex].RequestsLimit == 0 && iterator < count)
                    {
                    MoveNext();
                    iterator++;
                    }

                if (iterator < count)
                    {
                    credential =  credentials[currentIndex];
                    return true;
                    }
                else
                    {
                    credential =  null;
                    return false;
                    }
                }
            }

        private void MoveNext()
            {
            currentIndex++;
            if (currentIndex == count)
                currentIndex = 0;
            }
        }
    }
