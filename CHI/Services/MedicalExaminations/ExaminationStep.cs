using CHI.Models;
using System;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет информацию о шаге профилактического осмотра
    /// </summary>
    public class ExaminationStep : IEquatable<ExaminationStep>
    {
        /// <summary>
        /// Шаг прохождения профилактического осмотра
        /// </summary>
        public StepKind StepKind { get; set; }
        /// <summary>
        /// Дата прохождения
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Группа здоровья
        /// </summary>
        public HealthGroup HealthGroup { get; set; }
        /// <summary>
        /// Направление
        /// </summary>
        public Referral Referral { get; set; }

        public override bool Equals(object obj)
            => obj is ExaminationStep step && Equals(step);

        public bool Equals(ExaminationStep other)       
            => other !=null &&
            StepKind == other.StepKind &&
            Date == other.Date &&
            HealthGroup == other.HealthGroup &&
            Referral == other.Referral;

        public override int GetHashCode()
            => HashCode.Combine(StepKind, Date, HealthGroup, Referral);

    }
}
