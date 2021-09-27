using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class CasesIndicator : Indicator
    {
        public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
            => cases.Count;
    }
}
