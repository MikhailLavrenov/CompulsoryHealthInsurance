using PatientsFomsRepository.Models;
using System.Collections.Generic;

namespace FomsPatientsDB.Models
{
    public class Credential : BindableBase
    {
        #region Fields
        private readonly object locker = new object();
        private string login;
        private string password;
        private int requestsLimit;
        private int requestsLeft;
        #endregion

        #region Properties
        public string Login { get => login; set => SetProperty(ref login, value); }
        public string Password { get => password; set => SetProperty(ref password, value); }
        public int RequestsLimit
        {
            get => requestsLimit;
            set
            {
                SetProperty(ref requestsLimit, value);
                requestsLeft = value;
            }
        }
        #endregion

        #region Creator
        //создает копию экземпляра класса
        public Credential Copy()
        {
            return MemberwiseClone() as Credential;
        }
        #endregion

        #region Methods
        //попытка зарезервировать разрешение на запрос к серверу
        public bool TryReserveRequest()
        {
            lock (locker)
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
        //возвращает учетные данные по кругу пока лимит запросов не исчерпан
        #endregion

        public class RoundRobinCredentials
        {
            #region Fields
            private readonly object locker = new object();
            private int currentIndex;
            private List<Credential> credentials;
            #endregion

            #region Creator
            public RoundRobinCredentials(List<Credential> credentials)
            {
                this.credentials = new List<Credential>();
                credentials.ForEach(item => this.credentials.Add(item.Copy()));
                currentIndex = -1;
            }
            #endregion

            #region Methods
            //следующий элемент
            private void MoveNext()
            {
                if (currentIndex + 1 < credentials.Count)
                    currentIndex++;
                else
                    currentIndex = 0;
            }
            //попытка найти следующие учетные данные где лимит запросов не исчерпан
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
            #endregion
        }
    }
}
