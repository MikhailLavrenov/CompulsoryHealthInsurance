﻿using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class TreatmentPurposeCaseFiltersCollection : CaseFiltersCollectionBase
    {
        public override IEnumerable<Case> ApplyFilter(IEnumerable<Case> cases, int periodMonth, int periodYear)
        {
            var filterCodes = MatchCodesForPeriod(periodMonth, periodYear);
            return cases.Where(x => filterCodes.Contains(x.TreatmentPurpose));
        }
        public override string Description => "Цель обращения";
    }
}
