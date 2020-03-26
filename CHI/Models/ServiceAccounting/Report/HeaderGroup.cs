using CHI.Models.Infrastructure;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class HeaderGroup : IOrderedHierarchical<HeaderGroup>
    {
        public string Name { get; set; }
        public string SubName { get; set; }

        public bool IsRoot { get; set; }
        public int Order { get; set; }

        public List<HeaderItem> HeaderItems { get; set; }

        public HeaderGroup Parent { get; set; }
        public List<HeaderGroup> Childs { get; set; }
    }
}
