namespace CHI.Models.ServiceAccounting
{
    public class Indicator
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public IndicatorKind FacadeKind { get; set; }
        public IndicatorKind ValueKind { get; set; }
        public double MultiplicationFactor { get; set; } = 1;
        public double DivideFactor { get; set; } = 1;

        public Component Component { get; set; }
    }
}
