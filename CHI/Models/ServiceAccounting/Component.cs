using CHI.Models.Infrastructure;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Media;

namespace CHI.Models.ServiceAccounting
{
    public class Component :BindableBase, IOrderedHierarchical<Component>
    {
        string hexColor= "#FFFFFF";

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string HexColor { get=> hexColor; set=>SetProperty(ref hexColor, value); }
        public bool IsRoot { get; set; } = false;
        public bool IsCanPlanning { get; set; }
        public List<CaseFilter> CaseFilters { get; set; }
        public List<Indicator> Indicators { get; set; }

        public Component Parent { get; set; }
        public List<Component> Childs { get; set; }

    }
}
