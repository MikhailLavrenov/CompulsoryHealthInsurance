using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    class ReportViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Specialty> specialties;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Specialty> Specialties { get => specialties; set => SetProperty(ref specialties, value); }

        public ReportViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Отчет";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Departments.Include(x => x.Parameters).Include(x => x.Employees).ThenInclude(x => x.Parameters).Include(x=>x.Employees).ThenInclude(x=>x.Medic).Include(x=>x.Employees).ThenInclude(x=>x.Specialty).Load();
            dbContext.Components.Include(x=>x.Indicators).ThenInclude(x=>x.Expressions).Load();

            var report = new Report(dbContext.Departments.Local.First(x => x.IsRoot), dbContext.Components.Local.First(x => x.IsRoot));
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
