using CHI.Infrastructure;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Component : BindableBase, IHierarchical<Component>, IOrdered
    {
        string hexColor = "#FFFFFF";

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string HexColor { get => hexColor; set => SetProperty(ref hexColor, value); }
        public bool IsRoot { get; set; } = false;
        public bool IsCanPlanning { get; set; }
        public List<CaseFiltersCollection> CaseFilterCollections { get; set; }
        public List<Indicator> Indicators { get; set; }

        public Component Parent { get; set; }
        public List<Component> Childs { get; set; }

        public Component()
        {
            Childs = new List<Component>();
        }


        public List<Case> ApplyFilters(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            var result = cases;

            foreach (var caseFiltersCollection in CaseFilterCollections)
            {
                result = caseFiltersCollection.ApplyFilter(result, periodMonth, periodYear);
            }

            return result.ToList();
        }

    }
}
