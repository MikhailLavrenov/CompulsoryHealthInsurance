using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace CHI.ViewModels
{
    public class IndicatorsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Indicator> indicators;
        Component component;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Indicator> Indicators { get => indicators; set => SetProperty(ref indicators, value); }

        public IndicatorsViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Показатели";

            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            dbContext = new ServiceAccountingDBContext();

            component = navigationContext.Parameters.GetValue<Component>(nameof(Component));

            dbContext.Components.Where(x => x.Id == component.Id).Include(x => x.Indicators).Load();

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
