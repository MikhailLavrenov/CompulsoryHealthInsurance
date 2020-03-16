using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class IndicatorsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Indicator> indicators;
        Component component;

        public bool KeepAlive { get => false; }
        public Component Component { get => component; set => SetProperty(ref component, value); }
        public ObservableCollection<Indicator> Indicators { get => indicators; set => SetProperty(ref indicators, value); }
        public List<KeyValuePair<Enum, string>> IndicatorKinds { get; } = IndicatorKind.None.GetAllValuesAndDescriptions().ToList();

        public IndicatorsViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Показатели";


        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            dbContext = new ServiceAccountingDBContext();

            Component = navigationContext.Parameters.GetValue<Component>(nameof(Component));

            dbContext.Components.Where(x => x.Id == Component.Id).Include(x => x.Indicators).Load();

            Indicators = dbContext.Indicators.Local.ToObservableCollection();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            component.Indicators = Indicators.ToList();

            dbContext.SaveChanges();
        }

    }
}
