using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    class Patient
    {
        public int PatientId { get; set; }
        public string Initials { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public int Insurance { get; set; }

        public void SetInitialsFromFullName()
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

    }
}
