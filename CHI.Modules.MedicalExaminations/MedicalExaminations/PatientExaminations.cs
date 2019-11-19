using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.MedicalExaminations
{
    public class PatientExaminations
    {
        public string InsuranceNumber { get; }
        public List<Examination> Examinations { get; }
        public int Year { get; }
        public ExaminationKind ExaminationKind { get; }

        public PatientExaminations(string insuranceNumber, List<Examination> examinations)
        {
            InsuranceNumber = insuranceNumber;
            Examinations = examinations;

            Year = Examinations.First().Year;
            ExaminationKind = Examinations.First().Kind;
        }
        public PatientExaminations(string insuranceNumber, Examination examination)
            : this(insuranceNumber, new List<Examination> { examination })
        {
        }
    }
}
