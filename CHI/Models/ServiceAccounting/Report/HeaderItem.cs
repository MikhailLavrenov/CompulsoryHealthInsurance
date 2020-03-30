using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class HeaderItem
    {
        public string Name { get; set; }
        public int Position { get; set; } = -1;

        public HeaderGroup Group { get; set; }

        public HeaderItem(HeaderGroup headerGroup,string name)
        {
            Group = headerGroup;
            Name = name;
        }

        public static List<HeaderItem> CreateHeaderItems(HeaderGroup headerGroup, List<Indicator> indicators)
        {
            var items = new List<HeaderItem>();

            if (indicators?.Any() ?? false)
                foreach (var indicator in indicators)
                    items.Add(new HeaderItem(headerGroup, indicator.FacadeKind.GetDescription()));

            return items;
        }

        public static List<HeaderItem> CreateHeaderItems(HeaderGroup headerGroup, List<Parameter> parameters)
        {
            var items = new List<HeaderItem>();

            if (parameters?.Any() ?? false)
                foreach (var parameter in parameters)
                    items.Add(new HeaderItem(headerGroup,parameter.Kind.GetDescription()));

            return items;
        }
    }
}
