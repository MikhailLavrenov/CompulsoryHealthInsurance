using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{ 
    public class Case
    {
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор законченного случая из реестра
        /// </summary>
        public string IdCase { get; set; }
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
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateEnd { get; set; }
        /// <summary>
        /// Койко-дни
        /// </summary>
        public int BedDays { get; set; }
        /// <summary>
        /// Оплачено сумма
        /// </summary>
        public double AmountPaid { get; set; }
        /// <summary>
        /// Снято с оплаты сумма
        /// </summary>
        public double AmountUnpaid { get; set; }
        /// <summary>
        /// Статус оплаты
        /// </summary>
        public PaidKind PaidStatus { get; set; }
        public Employee Employee { get; set; }
        public List<Service> Services { get; set; }

    }
}
