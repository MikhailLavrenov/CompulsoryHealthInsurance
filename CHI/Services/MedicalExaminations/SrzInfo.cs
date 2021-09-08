namespace CHI.Services.MedicalExaminations
{
    public class SrzInfo
    {
        public int SrzPatientId { get; set; }
        public bool ExistInPlan { get; set; }
        /// <summary>
        /// Заполнены некоторые этапы. Может означать в т.ч. что пациент подан др ЛПУ.
        /// </summary>
        public bool FilledSomeStages { get; set; }
    }
}
