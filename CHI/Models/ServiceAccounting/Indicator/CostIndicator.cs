using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class CostIndicator : IndicatorBase
    {
        public override string Description => "Стоимость";
        public override string ShortDescription => "Стоим";


        //static CostIndicator()
        //{
        //    Description = "Стоимость";
        //    ShortDescription = "Стоим";
        //}


        protected override double CalculateCases(List<Case> cases, bool isPaymentAccepted)
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
