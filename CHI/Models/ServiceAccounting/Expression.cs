using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Expression
    {
        public int Id { get; set; }
        /// <summary>
        /// Цель посещения
        /// </summary>
        public double? VisitPurpose { get; set; }
        /// <summary>
        /// Цель обращения (выдумана ФОМСом)
        /// </summary>
        public int? TreatmentPurpose { get; set; }
        public double MultiplicationFactor { get; set; }
    }
}
