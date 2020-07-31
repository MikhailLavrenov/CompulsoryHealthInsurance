using System;

namespace CHI.Models.ServiceAccounting
{
    public class Parameter : ICloneable
    {
        public int Id { get; set; }
        public ParameterKind Kind { get; set; }
        public int Order { get; set; }

        public Department Department { get; set; }
        public Employee Employee { get; set; }

        public Parameter()
        {
        }

        public Parameter(int order, ParameterKind kind)
        {
            Order = order;
            Kind = kind;
        }

        public object Clone()
        {
            var copy= (Parameter)MemberwiseClone();
            copy.Id = 0;

            return copy;
        }
    }
}
