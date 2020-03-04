using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class MedicsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Medic> medics;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Medic> Medics { get => medics; set => SetProperty(ref medics, value); }

        public MedicsViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Медицинские работники";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();
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
