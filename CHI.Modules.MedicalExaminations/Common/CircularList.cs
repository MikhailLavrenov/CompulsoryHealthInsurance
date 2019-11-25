using System.Collections.Generic;

namespace CHI.Services.Common
{
    public class CircularList<T>
    {
        private readonly object locker = new object();
        protected IEnumerable<T> list;
        private IEnumerator<T> enumerator;

        public CircularList(IEnumerable<T> elements)
        {
            list = elements;
            enumerator = list.GetEnumerator();
        }
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
