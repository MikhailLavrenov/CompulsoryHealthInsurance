using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class LaborCostIndicator : Indicator
    {
        public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
            => cases.SelectMany(x => x.Services)
            .Where(x => x.ClassifierItem != null)
            .Sum(x => x.Count * x.ClassifierItem.LaborCost);
    }
}
