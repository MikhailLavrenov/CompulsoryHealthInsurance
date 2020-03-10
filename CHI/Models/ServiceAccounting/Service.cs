namespace CHI.Models.ServiceAccounting
{
    public class Service
    {       
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public double Count { get; set; }
        public int Code { get; set; }

        //string code;
        //public string Code { get => code; set => code = value?.ToUpper(); }
    }
}
