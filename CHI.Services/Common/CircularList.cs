using System.Collections.Generic;

namespace CHI.Services.Common
{
    public class CircularList<T>
    {
        private readonly object locker = new object();        
        private IEnumerator<T> enumerator;

        public IEnumerable<T> Elements { get; private set; }

        public CircularList(IEnumerable<T> elements)
        {
            Elements = elements;
            enumerator = Elements.GetEnumerator();
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
