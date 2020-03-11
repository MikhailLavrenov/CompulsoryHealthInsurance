using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public List<Indicator> Indicators { get; set; }
        public Component Parent { get; set; }
        public List<Component> Details { get; set; }
    }
}
