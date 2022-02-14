using CHI.Infrastructure;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Component : BindableBase, IHierarchical<Component>, IOrdered
    {
        string hexColor = "#FFFFFF";
        bool isTotal;


        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string HexColor { get => hexColor; set => SetProperty(ref hexColor, value); }
        public bool IsRoot { get; set; } = false;
        public bool IsCanPlanning { get; set; }
        public List<CaseFiltersCollectionBase> CaseFiltersCollections { get; set; }
        public List<IndicatorBase> Indicators { get; set; }
        public bool IsTotal { get => isTotal; set => SetProperty(ref isTotal, value); }
        public Component Parent { get; set; }
        public List<Component> Childs { get; set; }


        public Component()
        {
            Childs = new List<Component>();
            CaseFiltersCollections=new List<CaseFiltersCollectionBase>();
        }


        public List<Case> ApplyFilters(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            var result = cases;

            foreach (var caseFiltersCollection in CaseFiltersCollections)
            {
                result = caseFiltersCollection.ApplyFilter(result, periodMonth, periodYear);
            }

            return result.ToList();
        }

        public void AddCaseFilter(Type collectionType, CaseFilter filter)
        {
            var collection = CaseFiltersCollections.FirstOrDefault(x => x.GetType() == collectionType);

            if (collection == null)
            {
                collection = (CaseFiltersCollectionBase)Activator.CreateInstance(collectionType);
                CaseFiltersCollections.Add(collection);
            }

            collection.Filters.Add(filter);
        }

        public void RemoveCaseFilter(CaseFilter filter)
        {
            for (int i = 0; i < CaseFiltersCollections.Count; i++)
            {
               if( CaseFiltersCollections[i].Filters.Contains(filter))
                {
                    CaseFiltersCollections[i].Filters.Remove(filter);

                    if(CaseFiltersCollections[i].Filters.Count==0)
                        CaseFiltersCollections.RemoveAt(i);

                    break;
                }
            }
        }
    }
}
