using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Indicator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public IndicatorKind Kind { get; set; }
        public Component Component { get; set; }
        public List<Expression> Expressions { get; set; }
    }
}
