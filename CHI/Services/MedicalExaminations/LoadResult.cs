using CHI.Models;

namespace CHI.Services.MedicalExaminations
{
    public class LoadResult
    {
        public PatientExaminations PatientExaminations { get; set; }
        public bool IsLoaded { get; set; }
        public string ErrorMessage { get; set; }


        public LoadResult(PatientExaminations patientExaminations, bool isLoaded, string errorMessage)
        {
            PatientExaminations = patientExaminations;
            IsLoaded = isLoaded;
            ErrorMessage = errorMessage;
        }
    }
}
