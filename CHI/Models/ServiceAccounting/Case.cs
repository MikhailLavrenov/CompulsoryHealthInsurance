using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{ 
    public class Case
    {
        public int Id { get; set; }
        /// <summary>
        /// Код законченного случая в реестре
        /// </summary>
        public long CloseCaseCode { get; set; }
        /// <summary>
        /// Код случая в реестре
        /// </summary>
        public string IdCase { get; set; }
        /// <summary>
        /// Номер истории болезни в реестре
        /// </summary>
        public string MedicalHistoryNumber { get; set; }
        /// <summary>
        /// Идентификатор xml реестра
        /// </summary>
        public int BillRegisterCode { get; set; }
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
        /// Тип возраста
        /// </summary>
        public AgeKind AgeKind { get; set; }
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
