using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class VisitsIndicator : IndicatorBase
    {
        public override string Description => "Посещения";
        public override string ShortDescription => "Посещ";


        //static ServicesIndicator()
        //{
        //    Description = "Посещения";
        //    ShortDescription = "Посещ";
        //}


        //Надо переделать: рассчитывает из предположения что одна услуга основная (тарифная)
        //в стоматологии это не так, этот метод не применяется в стоматологии, пока...
        protected override double CalculateCases(List<Case> cases, bool isPaymentAccepted)
            => cases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
    }
}
