using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class ServiceClassifierItem
    {
        public int Id { get; set; }
        public int Code { get; set; }
        /// <summary>
        /// Условная единица труда (УЕТ)
        /// </summary>
        public double LaborCost { get; set; }
        public double Price { get; set; }
    }
}
