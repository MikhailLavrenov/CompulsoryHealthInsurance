namespace CHI.Models.ServiceAccounting
{
    public class Service
    {
        string code;

        public int Id { get; set; }
        public Employee Employee { get; set; }
        public double Count { get; set; }
        public string Code { get => code; set => code = value?.ToUpper(); }
    }
}
