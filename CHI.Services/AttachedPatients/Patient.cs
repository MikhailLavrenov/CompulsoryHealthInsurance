using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CHI.Services.AttachedPatients
{
    public class Patient
    {
        #region Свойства
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int PatientId { get; set; }
        [Key]
        public string InsuranceNumber { get; set; }
        public string Initials { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public bool FullNameExist { get; set; }
        #endregion

        #region Конструкторы
        public Patient(string insuranceNumber, string surname, string name, string patronymic)
        {
            InsuranceNumber = insuranceNumber;
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
            SetInitialsFromFullName();
            FullNameExist = true;
        }
        public Patient(string insuranceNumber, string initials)
        {
            InsuranceNumber = insuranceNumber;
            Initials = initials;
            FullNameExist = false;
        }
        public Patient() { }
        #endregion

        #region Методы
        //определяет инициалы по ФИО
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
        //Сравнивает экземпляры класса по значениям свойств
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
        //убирает лишние пробелы, приводит буквы к верхнему регистру, переопределяет инициалы
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
