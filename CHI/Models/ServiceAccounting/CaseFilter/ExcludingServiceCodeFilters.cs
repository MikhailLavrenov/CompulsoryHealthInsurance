using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class ExcludingServiceCodeFilters : CaseFiltersCollection
    {
        public override IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            throw new System.NotImplementedException();
        }
    }
}
