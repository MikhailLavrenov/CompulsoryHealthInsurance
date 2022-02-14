using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class CasesLaborCostIndicator : LaborCostIndicator
    {
        public override string Description => "Случаи (УЕТ)";
        public override string ShortDescription => "Cлуч";

    //    public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
    //=> cases.SelectMany(x => x.Services)
    //.Where(x => x.ClassifierItem != null)
    //.Sum(x => x.Count * x.ClassifierItem.LaborCost);
    }
}
