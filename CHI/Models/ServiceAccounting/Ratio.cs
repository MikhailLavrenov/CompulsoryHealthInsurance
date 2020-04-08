using System;

namespace CHI.Models.ServiceAccounting
{
    public class Ratio
    {
        public int Id { get; set; }

        public double Multiplier { get; set; } = 1;
        public double Divider { get; set; } = 1;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

    }
}
