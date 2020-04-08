using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class Indicator
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public IndicatorKind FacadeKind { get; set; }
        public IndicatorKind ValueKind { get; set; }
        public List<Ratio> Ratios { get; set; }

        public Component Component { get; set; }
    }
}
