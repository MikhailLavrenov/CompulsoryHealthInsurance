﻿using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class ColumnHeaderItem
    {
        public string Name { get; set; }
        public Indicator Indicator { get; set; }
        public int Priority { get; set; }
        //public int ColumnIndex { get; set; }
        public ColumnHeaderGroup Group { get; set; }



        public ColumnHeaderItem(ColumnHeaderGroup headerGroup, Indicator indicator)
        {
            Group = headerGroup;
            Indicator = indicator;
            Name= indicator.FacadeKind.GetDescription();
            Priority = headerGroup.Level + 1;
        }

    }
}
