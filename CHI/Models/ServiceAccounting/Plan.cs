using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Plan
    {
        public int Id { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
        public Indicator Indicator { get; set; }
        public Parameter Parameter { get; set; }
        public double Value { get; set; }
    }
}
