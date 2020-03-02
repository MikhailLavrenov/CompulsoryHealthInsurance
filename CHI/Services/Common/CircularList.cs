using System.Collections.Generic;

namespace CHI.Services.Common
{
    /// <summary>
    /// Возвращает элементы коллекции типа Т по замкнутому кругу по одному
    /// </summary>
    /// <typeparam name="T">Тип элементов</typeparam>
    public class CircularList<T>
    {
        private readonly object locker = new object();
        private IEnumerator<T> enumerator;

        /// <summary>
        /// Коллекция элементов
        /// </summary>
        public IEnumerable<T> Elements { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="elements"></param>
        public CircularList(IEnumerable<T> elements)
        {
            Elements = elements;
            enumerator = Elements.GetEnumerator();
        }

        /// <summary>
        /// Получить следующий элемент коллекции
        /// </summary>
        /// <returns></returns>
        public T GetNext()
        {
            lock (locker)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator.Reset();
                    enumerator.MoveNext();
                }

                return enumerator.Current;
            }
        }

    }
}
