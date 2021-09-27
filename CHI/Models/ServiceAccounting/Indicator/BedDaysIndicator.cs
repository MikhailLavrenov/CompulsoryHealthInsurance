using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class BedDaysIndicator : Indicator
    {
        public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
            => cases.Sum(x => x.BedDays);
    }
}
