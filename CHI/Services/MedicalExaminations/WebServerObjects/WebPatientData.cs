using CHI.Models;
using System;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет информацию о прохождении пациентом осмотров в конкретному году
    /// </summary>
    public class WebPatientData
    {
        /// <summary>
        /// Id пациента в портале диспансеризации
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Id пациента в СРЗ
        /// </summary>
        public int PersonId { get; set; }
        /// <summary>
        /// Дата начала 1ого этапа
        /// </summary>
        public DateTime? Disp1BeginDate { get; set; }
        /// <summary>
        /// Дата окончания 1ого этапа
        /// </summary>
        public DateTime? Disp1Date { get; set; }
        /// <summary>
        /// Группа здоровья 1ого этапа
        /// </summary>
        public HealthGroup? Stage1ResultId { get; set; }
        /// <summary>
        /// Направление по результатам 1ого этапа
        /// </summary>
        public Referral? Stage1DestId { get; set; }
        /// <summary>
        /// Дата направления на 2ой этап
        /// </summary>
        public DateTime? Disp2DirectDate { get; set; }
        /// <summary>
        /// Дата начала 2ого этапа
        /// </summary>
        public DateTime? Disp2BeginDate { get; set; }
        /// <summary>
        /// Дата окончания 2ого этапа
        /// </summary>
        public DateTime? Disp2Date { get; set; }
        /// <summary>
        /// Группа здоровья 2ого этапа
        /// </summary>
        public HealthGroup? Stage2ResultId { get; set; }
        /// <summary>
        /// Направление по результатам 2ого этапа
        /// </summary>
        public Referral? Stage2DestId { get; set; }
        ///// <summary>
        ///// Итоговая дата завершения прохождения осмотра 
        ///// </summary>
        //public DateTime? DispSuccessDate { get; set; }
        /// <summary>
        /// Дата отказа от прохождения осмотра
        /// </summary>
        public DateTime? DispCancelDate { get; set; }
        /// <summary>
        /// Вид осмотра
        /// </summary>
        public ExaminationKind DispType { get; set; }
        public int YearId { get; set; }
    }
}
