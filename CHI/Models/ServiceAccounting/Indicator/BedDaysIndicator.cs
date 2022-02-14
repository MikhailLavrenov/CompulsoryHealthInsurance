using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class BedDaysIndicator : IndicatorBase
    {
        public override string Description => "Койко-дни";
        public override string ShortDescription => "КДн";


        //static BedDaysIndicator()
        //{
        //    Description = "Койко-дни";
        //    ShortDescription = "КДн";
        //}


        protected override double CalculateCases(List<Case> cases, bool isPaymentAccepted)
            => cases.Sum(x => x.BedDays);
    }
}