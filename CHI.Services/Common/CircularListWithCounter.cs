using CHI.Services.Common;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Common
{
    /// <summary>
    /// Возвращает элементы коллекции типа Т по замкнутому кругу по одному пока лимит запросов не исчерпан
    /// </summary>
    public class CircularListWithCounter<T> where T: class
    {
        #region Поля
        private readonly object locker = new object();
        private uint totalCounter;
        private CircularList<CountableObject> circularList;
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="keyValuePairs">Универсальная коллекция объектов "ключ-значение", где ключ - элемент коллекции, значение - лимит счетчика </param>
        public CircularListWithCounter(IDictionary<T,uint> keyValuePairs)
        {
            var countableObjects = new List<CountableObject>();
            totalCounter = 0;

            foreach (var keyValuePair in keyValuePairs)
            {
                countableObjects.Add(new CountableObject(keyValuePair.Key, keyValuePair.Value));
                totalCounter += keyValuePair.Value;
            }

            circularList = new CircularList<CountableObject>(countableObjects);
        }
        #endregion

        #region Методы
        /// <summary>
        /// Попытаться зарезервировать номер экземпляра типа по замкнутому кругу и уменьшить его счетчик на 1. 
        /// В случае успеха возращает true и уменьшает счетчик на 1.
        /// В случае неудачи возвращает false.
        /// </summary>
        /// <param name="element">Ссылка на экземпляр у которого уменьшается единица счетчика.</param>
        /// <returns>Результат операции. True - успешно, False - неудачно.</returns>
        public bool TryReserve(T element)
        {
            lock (locker)
            {
                if (element == null)
                    return false;

                var countableObject = circularList.Elements.FirstOrDefault(x => x.Object == element);

                if (countableObject == null || countableObject.Counter == 0)
                    return false;

                countableObject.Counter--;
                totalCounter--;
                return true;
            }
        }
        /// <summary>
        /// Попытаться получить следующий экземпляр коллекции и уменьшить его счетчик на 1. 
        /// В случае успеха возращает true и уменьшает счетчик на 1.
        /// В случае неудачи возвращает false.
        /// </summary>
        /// <param name="element">Следующий эелемент коллекции у которого удалось уменьшить счетчик на 1.
        /// В случае неудачи - null.</param>
        /// <returns>Результат операции. True - успешно, False - неудачно.</returns>
        public bool TryGetNext(out T element)
        {
            lock (locker)
            {
                while (totalCounter > 0)
                {
                    var countCredential = circularList.GetNext();

                    if (countCredential.Counter != 0)
                    {
                        countCredential.Counter--;
                        totalCounter--;
                        element = countCredential.Object;
                        return true;
                    }
                }

                element = null;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Представляет экземпляр типа T и его текущее значение счетчика.
        /// </summary>
        private class CountableObject
        {
            /// <summary>
            /// Экземляр типа Т
            /// </summary>
            public T Object { get; private set; }
            /// <summary>
            /// Текущее значение счетчика
            /// </summary>
            public uint Counter { get; set; }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="obj">Экземпляр типа Т</param>
            /// <param name="initialCounter">Значение счетчика</param>
            public CountableObject(T obj, uint initialCounter)
            {
                Object = obj;
                Counter = initialCounter;
            }
        }
    }
}
