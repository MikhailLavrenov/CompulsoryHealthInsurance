using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public abstract class IndicatorBase : BindableBase
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public List<Ratio> Ratios { get; set; }
        public abstract string Description { get; }
        public abstract string ShortDescription { get; }

        public Component Component { get; set; }


        protected abstract double CalculateCases(List<Case> cases, bool isPaymentAccepted);

        public double CalculateValue(List<Case> cases, bool isPaymentAccepted, int periodMonth, int periodYear)
        {
            var casesValue = CalculateCases(cases, isPaymentAccepted);
            return ApplyRatio(casesValue, periodMonth, periodYear);
        }

        protected double ApplyRatio(double value, int periodMonth, int periodYear)
            => Ratios.FirstOrDefault(x => x.IsValidForPeriod(periodMonth, periodYear))?.Apply(value) ?? value;
    }
}
