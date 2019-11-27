namespace CHI.Services.MedicalExaminations
{
    public class PatientExaminations
    {
        private Examination stage1;
        private Examination stage2;

        public string InsuranceNumber { get; set; }
        public Examination Stage1 { get => stage1; set => stage1 = value; }
        public Examination Stage2 { get => stage2; set => stage2 = value; }
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
