using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHI.Services.AttachedPatients
{
    /// <summary>
    /// Представляет сведения о прикрепленном пациенте
    /// </summary>
    public class Patient
    {
        #region Свойства
        /// <summary>
        /// Серия и/или номер полиса
        /// </summary>
        [Key]
        public string InsuranceNumber { get; set; }
        /// <summary>
        /// Инициалы ФИО
        /// </summary>
        public string Initials { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string Surname { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        public string Patronymic { get; set; }
        /// <summary>
        /// Имеется полное ФИО
        /// </summary>
        public bool FullNameExist { get; set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса</param>
        /// <param name="surname">Фамилия</param>
        /// <param name="name">Имя</param>
        /// <param name="patronymic">Отчество</param>
        public Patient(string insuranceNumber, string surname, string name, string patronymic)
        {
            InsuranceNumber = insuranceNumber;
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
            SetInitialsFromFullName();
            FullNameExist = true;
        }
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса/</param>
        /// <param name="initials">Инициалы ФИО/</param>
        public Patient(string insuranceNumber, string initials)
        {
            InsuranceNumber = insuranceNumber;
            Initials = initials;
            FullNameExist = false;
        }
        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Patient() { }
        #endregion

        #region Методы
        /// <summary>
        /// определяет инициалы по полному ФИО/
        /// </summary>
        private void SetInitialsFromFullName()
        {
            var str = new StringBuilder();

            if (Surname != null && Surname.Length > 0)
                str.Append(Surname[0]);
            if (Name != null && Name.Length > 0)
                str.Append(Name[0]);
            if (Patronymic != null && Patronymic.Length > 0)
                str.Append(Patronymic[0]);

            Initials = str.ToString().ToUpper();
        }
        /// <summary>
        /// Сравнивает экземпляры класса по значениям свойств
        /// </summary>
        /// <param name="patient">Ссылка на экземпляр с которым сравнивается/</param>
        /// <returns>True-совпадают, False-не совпадают.</returns>
        public bool Equals(Patient patient)
        {
            if (InsuranceNumber.Equals(patient.InsuranceNumber, StringComparison.OrdinalIgnoreCase)
                && Surname.Equals(patient.Surname, StringComparison.OrdinalIgnoreCase)
                && Name.Equals(patient.Name, StringComparison.OrdinalIgnoreCase)
                && Patronymic.Equals(patient.Patronymic, StringComparison.OrdinalIgnoreCase))
                return true;
            else
                return false;

        }
        /// <summary>
        /// убирает лишние пробелы, приводит буквы к верхнему регистру, переопределяет инициалы/
        /// </summary>
        public void Normalize()
        {
            InsuranceNumber = InsuranceNumber.Replace(" ", "").ToUpper();
            Surname = Surname.Replace("  ", " ").Trim().ToUpper();
            Name = Name.Replace("  ", " ").Trim().ToUpper();
            Patronymic = Patronymic.Replace("  ", " ").Trim().ToUpper();
            SetInitialsFromFullName();
        }
        #endregion
    }
}
