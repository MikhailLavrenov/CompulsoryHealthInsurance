using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class ServicesIndicator : Indicator
    {
        //Надо переделать: рассчитывает из предположения что одна услуга основная (тарифная)
        //в стоматологии это не так, этот метод не применяется в стоматологии, пока...
        public override double CalculateValue(List<Case> cases, bool isPaymentAccepted)
            => cases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
    }
}
