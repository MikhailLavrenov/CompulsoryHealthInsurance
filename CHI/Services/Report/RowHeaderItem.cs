using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class RowHeaderItem
    {
        public string Name { get; set; }
        public Parameter Parameter { get; set; }
        public int Priority { get; set; }
        public int Index { get; set; }
        public RowHeaderGroup Group { get; set; }



        public RowHeaderItem(RowHeaderGroup headerGroup, Parameter parameter)
        {            
            Group = headerGroup;
            Parameter = parameter;
            Name = parameter.Kind.GetShortDescription();
            Priority = headerGroup.Level + 1;
        }




    }
}
