using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;

namespace PatientsFomsRepository.Models
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

    }
}
