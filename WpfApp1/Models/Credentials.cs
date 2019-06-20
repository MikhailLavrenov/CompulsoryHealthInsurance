using System.Collections.Generic;
using System.Linq;

namespace FomsPatientsDB.Models
    {

    //возввращает учтеные данные по кругу пока лимит запросов не исчерпан
    class RoundRobinCredentials
        {
        private List<Credential> credentials;
        private int currentIndex;
        private int count;

        public Credential Current { get; private set; }

        public RoundRobinCredentials(List<Credential> credentials)
            {
            this.credentials = new List<Credential>();
            credentials.ForEach(x => this.credentials.Add(x.Copy()));
            currentIndex = -1;
            count = credentials.Count;
            totalRequestsLeft = credentials.Select(x => x.RequestsLeft).Sum();
            }

        public bool TryMoveNext()
            {            
            MoveNext();

            int iterator = 0;
            while (Current.RequestsLeft == 0 && iterator < count)
                {
                MoveNext();
                iterator++;
                }

            return iterator<count ? true :false;
            }

        private void MoveNext()
            {
            currentIndex++;
            if (currentIndex == count)
                currentIndex = 0;

            Current = credentials[currentIndex];
            }
        }
    }
