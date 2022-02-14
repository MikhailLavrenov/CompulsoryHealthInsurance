using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class ServiceCodeCaseFiltersCollection : CaseFiltersCollectionBase
    {
        public override IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            var filterCodes = MatchCodesForPeriod(periodMonth, periodYear);
            return cases.Where(x => x.Services.Any(y => filterCodes.Contains(y.Code)));
        }
        public override string Description => "Содержит услугу";
    }
}
