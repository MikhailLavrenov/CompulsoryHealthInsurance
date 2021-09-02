using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public abstract class CaseFiltersCollection
    {
        public List<CaseFilter> Filters { get; set; }


        public CaseFiltersCollection()
        {
            Filters = new List<CaseFilter>();
        }

        public abstract IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear);

        protected IEnumerable<double> MatchCodesForPeriod(int periodMonth, int periodYear)
            => Filters.Where(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, periodMonth, periodYear)).Select(x=>x.Code);
    }
}
