using CHI.Models;
using System;
using System.Collections.Generic;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет информацию о шаге профилактического осмотра
    /// </summary>
    public class ExaminationStep : IEqualityComparer<ExaminationStep>
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

        /// <summary>
        /// Определяет, равны ли два указанных объекта.
        /// </summary>
        /// <param name="x">Первый объект типа T для сравнения.</param>
        /// <param name="y">Второй объект типа T для сравнения.</param>
        /// <returns>true, если указанные объекты равны; в противном случае — false.</returns>
        public bool Equals(ExaminationStep x, ExaminationStep y)
        {
            if (x.Equals(y))
                return true;

            if (x == null && y != null)
                return false;

            if (x != null && y == null)
                return false;

            return x.StepKind.Equals(y.StepKind)
                && x.Date.Equals(y.Date)
                && x.HealthGroup.Equals(y.HealthGroup)
                && x.Referral.Equals(y.Referral);
        }
        /// <summary>
        /// Возвращает хэш-код указанного объекта.
        /// </summary>
        /// <param name="obj">"Экземпляр для которого должен быть возвращен хэш-код.</param>
        /// <returns>Хэш-код указанного объекта.</returns>
        public int GetHashCode(ExaminationStep obj)
        {
            unchecked
            {
                // Выбираем большие простые числа, чтобы избежать коллизий хэширования
                int HashingBase = (int)2166136261;
                int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ StepKind.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Date.GetHashCode();
                hash = (hash * HashingMultiplier) ^ HealthGroup.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Referral.GetHashCode();

                return hash;
            }
        }
    }
}
