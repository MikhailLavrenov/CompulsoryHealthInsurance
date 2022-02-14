using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class LaborCostIndicator : IndicatorBase
    {
        public override string Description => "УЕТ";
        public override string ShortDescription => "УЕТ";


        //static LaborCostIndicator()
        //{
        //    Description = "УЕТ";
        //    ShortDescription = "УЕТ";
        //}


        protected override double CalculateCases(List<Case> cases, bool isPaymentAccepted)
            => cases.SelectMany(x => x.Services)
            .Where(x => x.ClassifierItem != null)
            .Sum(x => x.Count * x.ClassifierItem.LaborCost);
    }
}
