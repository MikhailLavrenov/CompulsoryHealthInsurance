using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Models
    {
    /// <summary>
    /// возвращает учетные данные по кругу пока лимит запросов не исчерпан
    /// </summary>
    public class RoundRobinCredentials
        {
        #region Поля
        private readonly object locker = new object();
        private int currentIndex;
        private List<Credential> credentials;
        #endregion

        #region Конструкторы
        public RoundRobinCredentials(List<Credential> credentials)
            {
            this.credentials = new List<Credential>();
            credentials.ForEach(item => this.credentials.Add(item.Copy()));
            currentIndex = -1;
            }
        #endregion

        #region Методы
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
