using CHI.Services.Common;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Common
{
    /// <summary>
    /// возвращает объекты типа T по кругу пока лимит запросов не исчерпан
    /// </summary>
    public class CircularListWithCounter<T> where T: class
    {
        #region Поля
        private readonly object locker = new object();
        private uint totalCounter;
        private CircularList<CountableObject> circularList;
        #endregion

        #region Конструкторы
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

        private class CountableObject
        {
            public T Object { get; private set; }
            public uint Counter { get; set; }

            public CountableObject(T obj, uint initialCounter)
            {
                Object = obj;
                Counter = initialCounter;
            }
        }
    }
}
