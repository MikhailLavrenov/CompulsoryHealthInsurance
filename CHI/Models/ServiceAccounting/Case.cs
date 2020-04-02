using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{ 
    public class Case
    {
        public int Id { get; set; }
        /// <summary>
        /// Условия оказания
        /// </summary>
        public int Place { get; set; }
        /// <summary>
        /// Цель посещения
        /// </summary>
        public double VisitPurpose { get; set; }
        /// <summary>
        /// Цель обращения (выдумана ФОМСом)
        /// </summary>
        public int TreatmentPurpose { get; set; }
        public int BedDays { get; set; }
        public Employee Employee { get; set; }
        public List<Service> Services { get; set; }

    }
}
