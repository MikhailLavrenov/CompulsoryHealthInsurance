using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Parameter
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
    }
}
