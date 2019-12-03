using System;

namespace CHI.Services.MedicalExaminations
{
    public class PatientExaminations
    {
        public string InsuranceNumber { get; set; }
        public string Initials { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public DateTime Birthdate { get; set; }
        public Examination Stage1 { get; set; }
        public Examination Stage2 { get; set; }
        public int Year { get; private set; }
        public ExaminationKind Kind { get; private set; }

        public PatientExaminations(string insuranceNumber, int year, ExaminationKind examinationKind)
        {
            InsuranceNumber = insuranceNumber;
            Year = year;
            Kind = examinationKind;

        }

        //public PatientExaminations(string insuranceNumber, List<Examination> examinations)
        //{
        //    InsuranceNumber = insuranceNumber;
        //    Examinations = examinations;

        //    Year = Examinations.First().Year;
        //    ExaminationKind = Examinations.First().Kind;
        //}
        //public PatientExaminations(string insuranceNumber, Examination examination)
        //    : this(insuranceNumber, new List<Examination> { examination })
        //{
        //}
    }
}
