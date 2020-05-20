using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHI.Models
{
    /// <summary>
    /// Представляет сведения о прикрепленном пациенте
    /// </summary>
    public class Patient
    {
        #region Поля
        public string insuranceNumber;
        public string initials;
        #endregion

        #region Свойства
        /// <summary>
        /// Серия и/или номер полиса
        /// </summary>
        public int Id { get; set; }
        public string InsuranceNumber { get=> insuranceNumber; set=> insuranceNumber=value.ToUpper(); }
        /// <summary>
        /// Инициалы ФИО
        /// </summary>
        public string Initials { get => initials; set => initials = value.ToUpper(); }
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
            DefineInitilas();
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
        public Patient()
        { }
        #endregion

        #region Методы
        /// <summary>
        /// определяет инициалы по полному ФИО/
        /// </summary>
        public void DefineInitilas()
        {
            var str = new StringBuilder();

            if (Surname != null && Surname.Length > 0)
                str.Append(Surname[0]);
            if (Name != null && Name.Length > 0)
                str.Append(Name[0]);
            if (Patronymic != null && Patronymic.Length > 0)
                str.Append(Patronymic[0]);

            Initials = str.ToString();
        }
         #endregion
    }
}
