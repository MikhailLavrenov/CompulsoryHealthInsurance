namespace CHI.Models.ServiceAccounting
{
    public class FlkCaseEntry
    {
        public int BillRegisterCode { get; }
        public long CloseCaseCode { get; }
        public string MedicalHistoryNumber { get; }
        public bool IsRejected { get; }


        public FlkCaseEntry(int billRegisterCode, long closeCaseCode, string medicalHistoryNumber, bool isRejected)
        {
            BillRegisterCode = billRegisterCode;
            CloseCaseCode = closeCaseCode;
            MedicalHistoryNumber = medicalHistoryNumber;
            IsRejected = isRejected;
        }
    }
}
