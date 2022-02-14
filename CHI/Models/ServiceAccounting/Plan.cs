namespace CHI.Models.ServiceAccounting
{
    public class Plan
    {
        public int Id { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
        public IndicatorBase Indicator { get; set; }
        public Parameter Parameter { get; set; }
        public double Value { get; set; }

        public Plan()
        {
        }
        public Plan(IndicatorBase indicator, Parameter parameter)
        {
            Indicator = indicator;
            Parameter = parameter;
        }
    }
}
