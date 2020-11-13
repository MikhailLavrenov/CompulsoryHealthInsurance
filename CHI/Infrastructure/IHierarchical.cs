using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Infrastructure
{
    public interface IHierarchical<T> where T : class
    {
        public T Parent { get; set; }
        public List<T> Childs { get; set; }
    }
}
