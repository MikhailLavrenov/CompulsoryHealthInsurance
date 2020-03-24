namespace CHI.Models.ServiceAccounting
{
    public class Expression
    {
        public int Id { get; set; }

        public bool IsCommonFilter { get; set; }
        public ExpressionKind Kind { get; set; }
        public string Value { get; set; }        
        public double MultiplicationFactor { get; set; } = 1;
    }
}
