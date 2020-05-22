using Prism.Mvvm;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class Indicator : BindableBase
    {
        IndicatorKind faceKind;
        IndicatorKind valueKind;

        public int Id { get; set; }
        public int Order { get; set; }
        public IndicatorKind FacadeKind { get => faceKind; set { faceKind = value; ValueKind = value; } }
        public IndicatorKind ValueKind { get => valueKind; set => SetProperty(ref valueKind, value); }
        public List<Ratio> Ratios { get; set; }

        public Component Component { get; set; }
    }
}
