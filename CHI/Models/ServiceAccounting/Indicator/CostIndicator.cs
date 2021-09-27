using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class CostIndicator : Indicator
    {
        public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
        {
            if (isPaymentAccepted)
                return cases
                    .Where(x => x.PaidStatus == PaidKind.None)
                    .SelectMany(x => x.Services)
                    .Where(x => x.ClassifierItem != null)
                    .Sum(x => x.Count * x.ClassifierItem.Price)
                    + cases
                    .Where(x => x.PaidStatus != PaidKind.None)
                    .Sum(x => x.AmountPaid);
            else
                return cases.Sum(x => x.AmountUnpaid);
        }
    }
}
