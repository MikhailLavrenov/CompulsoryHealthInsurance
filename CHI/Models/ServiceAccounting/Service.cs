using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Service
    {
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public double Count { get; set; }
        public string Code { get; set; }
    }
}
