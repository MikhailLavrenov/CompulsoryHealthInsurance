using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class ValueItem
    {
        public int Id { get; set; }
        public Parameter Parameter { get; set; }
        public Indicator Indicator { get; set; }
        public double Value { get; set; }
    }
}
