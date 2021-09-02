using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class ServiceCodeFilters : CaseFiltersCollection
    {
        public override IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            throw new System.NotImplementedException();
        }
    }
}
