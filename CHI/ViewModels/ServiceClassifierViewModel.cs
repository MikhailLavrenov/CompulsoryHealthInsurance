using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    class ServiceClassifierViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<ServiceClassifier> serviceClassifier;

        public bool KeepAlive { get => false; }
        public ObservableCollection<ServiceClassifier> ServiceClassifier { get => serviceClassifier; set => SetProperty(ref serviceClassifier, value); }

        public ServiceClassifierViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Классификатор Услуг";

            dbContext = new ServiceAccountingDBContext();
            dbContext.ServiceClassifier.Load();
            ServiceClassifier = dbContext.ServiceClassifier.Local.ToObservableCollection();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            dbContext.SaveChanges();
        }
    }
}
