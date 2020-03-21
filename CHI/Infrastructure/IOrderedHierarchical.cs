using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.Infrastructure
{
    public interface IOrderedHierarchical<T> where T : class
    {
        public int Order { get; set; }

        public T Parent { get; set; }
        public List<T> Childs { get; set; }
    }
}
