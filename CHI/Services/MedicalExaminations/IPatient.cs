using System;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет информациию о пациенте
    /// </summary>
    public interface IPatient
    {
        /// <summary>
        /// Фамилия
        /// </summary>
        string Surname { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        string Patronymic { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        DateTime Birthdate { get; set; }
    }
}
