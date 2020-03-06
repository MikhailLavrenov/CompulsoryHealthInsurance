namespace CHI.Models.ServiceAccounting
{
    public class Specialty
    {
        public int? Id { get; set; }
        public int FomsId { get; set; }
        public string Title { get; set; }

        public static Specialty CreateUnknown(int fomsId)
        {
            return new Specialty
            {
                FomsId = fomsId,
                Title = "Неизвестно"
            };
        }
    }
}
