using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class VisitPurposeFilters : CaseFiltersCollection
    {
        public override IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear)        
            => MatchCodesForPeriod(periodMonth, periodYear).Join(cases, f => f, c => c.VisitPurpose, (f, c) => c);
    }
}