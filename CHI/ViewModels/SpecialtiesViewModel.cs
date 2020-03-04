using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class SpecialtiesViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Specialty> specialties;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Specialty> Specialties { get => specialties; set => SetProperty(ref specialties, value); }

        public SpecialtiesViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Специальности";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Specialties.Load();
            Specialties = dbContext.Specialties.Local.ToObservableCollection();
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
