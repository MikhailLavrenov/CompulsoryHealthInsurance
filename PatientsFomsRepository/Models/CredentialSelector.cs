using System.Collections.Generic;

namespace PatientsFomsRepository.Models
{
    /// <summary>
    /// возвращает учетные данные по кругу пока лимит запросов не исчерпан
    /// </summary>
    public class CredentialSelector
    {
        #region Поля
        private readonly object locker = new object();
        private Dictionary<Credential, uint> credentials;
        private uint TotalCounter;
        #endregion

        #region Конструкторы
        public CredentialSelector(IEnumerable<Credential> creds)
        {
            credentials = new Dictionary<Credential, uint>();
            TotalCounter = 0;

            foreach (var credential in creds)
            {
                credentials.Add(credential, credential.RequestsLimit);
                TotalCounter += credential.RequestsLimit;
            }
        }
        #endregion

        #region Методы
        #endregion
    }
}
