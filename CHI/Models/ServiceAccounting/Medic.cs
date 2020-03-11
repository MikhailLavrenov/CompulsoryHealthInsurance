namespace CHI.Models.ServiceAccounting
{
    public class Medic
    {
        string fomsId;

        public int Id { get; set; }
        public string FomsId { get => fomsId; set => fomsId = value?.ToUpper(); }
        public string FullName { get; set; }

        public static Medic CreateUnknown(string fomsId)
        {
            return new Medic
            {
                FomsId = fomsId,
                FullName = "Неизвестно"
            };
        }
    }
}
