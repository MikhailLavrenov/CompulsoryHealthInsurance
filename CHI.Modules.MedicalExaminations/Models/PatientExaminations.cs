using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Modules.MedicalExaminations.Models
{
    public class PatientExaminations
    {
        public string InsuranceNumber { get; set; }
        public List<Examination> Examinations { get; set; }

        public PatientExaminations()
        {
            Examinations = new List<Examination>();
        }
    }
}
