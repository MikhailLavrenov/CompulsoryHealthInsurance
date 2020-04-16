using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class ColumnHeaderItem:BindableBase
    {
        public string Name { get; set; }
        public Indicator Indicator { get; set; }
        public int Priority { get; set; }
        public int Index { get; set; }
        public ColumnHeaderGroup Group { get; set; }



        public ColumnHeaderItem(ColumnHeaderGroup headerGroup, Indicator indicator)
        {
            Group = headerGroup;
            Indicator = indicator;
            Name= indicator.FacadeKind.GetShortDescription();
            Priority = headerGroup.Level + 1;
        }

    }
}
