using System;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет медицинский профилактический осмотр
    /// </summary>
    public class Examination
    {
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime BeginDate { get; set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// Группа здоровья
        /// </summary>
        public HealthGroup HealthGroup { get; set; }
        /// <summary>
        /// Направление
        /// </summary>
        public Referral Referral { get; set; }
    }
}
