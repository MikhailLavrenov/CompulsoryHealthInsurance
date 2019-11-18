using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    public class Patient:IEqualityComparer<Patient>
    {
        public string InsuranceNumber { get; set; }

        public Patient(string insuranceNumber)
        {
            InsuranceNumber = insuranceNumber;
        }

        public bool Equals(Patient x, Patient y)
        {
            return x.InsuranceNumber.Equals(y.InsuranceNumber, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Patient obj)
        {
            return obj.InsuranceNumber.GetHashCode();
        }
    }
}
