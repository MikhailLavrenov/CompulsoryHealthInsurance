using PatientsFomsRepository.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PatientsFomsRepository.Models
{
    public class Patient : BindableBase
    {
        #region Поля
        private string insuranceNumber;
        private string initials;
        private string surname;
        private string name;
        private string patronymic;
        private bool fullNameExist;
        #endregion

        #region Свойства
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int PatientId { get; set; }
        [Key]
        public string InsuranceNumber { get => insuranceNumber; set => SetProperty(ref insuranceNumber, value); }
        public string Initials { get => initials; set => SetProperty(ref initials, value); }
        public string Surname { get => surname; set => SetProperty(ref surname, value); }
        public string Name { get => name; set => SetProperty(ref name, value); }
        public string Patronymic { get => patronymic; set => SetProperty(ref patronymic, value); }
        public bool FullNameExist { get => fullNameExist; set => SetProperty(ref fullNameExist, value); }
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
            if (InsuranceNumber == patient.InsuranceNumber
                && Surname == patient.Surname
                && Name == patient.Name
                && Patronymic == patient.Patronymic)
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
