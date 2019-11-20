using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.Common
{
    public class CircularList<T>
    { 
        private IEnumerable<T> list;
        private IEnumerator<T> enumerator;

        public CircularList(IEnumerable<T> elements)
        {
            list = elements;
            enumerator = list.GetEnumerator();
        }
        public T GetNext()
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
