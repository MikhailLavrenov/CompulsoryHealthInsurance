using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public abstract class CaseFiltersCollectionBase
    {
        public int Id { get; set; }
        public List<CaseFilter> Filters { get; set; }
        public abstract string Description { get; }


        public CaseFiltersCollectionBase()
        {
            Filters = new List<CaseFilter>();
        }


        public abstract IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear);

        protected List<double> MatchCodesForPeriod(int periodMonth, int periodYear)
            => Filters.Where(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, periodMonth, periodYear)).Select(x=>x.Code).ToList();
    }
}
