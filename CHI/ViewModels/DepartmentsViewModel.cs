using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class DepartmentsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Department> departments;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Department> Departments { get => departments; set => SetProperty(ref departments, value); }

        public DepartmentsViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Подразделения";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Departments.Load();
            Departments = dbContext.Departments.Local.ToObservableCollection();
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
