using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class CasesIndicator : IndicatorBase
    {
        public override string Description => "Cлучаи";
        public override string ShortDescription => "Cлуч";


        //static CasesIndicator()
        //{
        //    Description = "Cлучаи";
        //    ShortDescription = "Cлуч";
        //}


        protected override double CalculateCases(List<Case> cases, bool isPaymentAccepted)
            => cases.Count;
    }
}
