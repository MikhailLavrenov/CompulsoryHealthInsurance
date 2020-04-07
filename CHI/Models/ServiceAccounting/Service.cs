using System;

namespace CHI.Models.ServiceAccounting
{
    public class Service
    {       
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public double Count { get; set; }
        public int Code { get; set; }
        public DateTime Date { get; set; }
    }
}
