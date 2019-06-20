using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WpfApp1.Models
    {
    public class Patient
        {
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
        public Patient() {}

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

        public bool Equals(Patient patient)
            {
            if (InsuranceNumber==patient.InsuranceNumber
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
        }
    }
