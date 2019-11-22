using System.Collections.Generic;

namespace CHI.Services.SRZ
{
    /// <summary>
    /// возвращает учетные данные по кругу пока лимит запросов не исчерпан
    /// </summary>
    public class CircularCredentials
    {
        #region Поля
        private readonly object locker = new object();
        private int currentIndex;
        private List<Credential> credentials;
        #endregion

        #region Конструкторы
        public CircularCredentials(IEnumerable<Credential> credentials)
        {
            this.credentials = new List<Credential>();

            foreach (var item in credentials)
            {
                var itemCopy = item.Copy();
                this.credentials.Add(itemCopy);
            }
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
