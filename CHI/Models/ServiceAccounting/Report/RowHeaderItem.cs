using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class RowHeaderItem
    {
        public string Name { get; set; }
        public Parameter Parameter { get; set; }
        public int Priority { get; set; }
        //public int RowIndex { get; set; }
        public RowHeaderGroup Group { get; set; }



        public RowHeaderItem(RowHeaderGroup headerGroup, Parameter parameter)
        {            
            Group = headerGroup;
            Parameter = parameter;
            Name = parameter.Kind.GetDescription();
            Priority = headerGroup.Level + 1;
        }




    }
}
