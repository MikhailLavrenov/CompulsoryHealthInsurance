using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class EmployeesViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Employee> employees;
        ObservableCollection<Medic> medics;
        ObservableCollection<Specialty> specialties;
        ObservableCollection<Department> departments;

        public bool KeepAlive { get => false; }
        public ObservableCollection<Employee> Employees { get => employees; set => SetProperty(ref employees, value); }
        public ObservableCollection<Medic> Medics { get => medics; set => SetProperty(ref medics, value); }
        public ObservableCollection<Specialty> Specialties { get => specialties; set => SetProperty(ref specialties, value); }
        public ObservableCollection<Department> Departments { get => departments; set => SetProperty(ref departments, value); }

        public EmployeesViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Штатные единицы";

            dbContext = new ServiceAccountingDBContext();

            dbContext.Employees.Load();
            Employees = dbContext.Employees.Local.ToObservableCollection();

            dbContext.Medics.Load();
            Medics = dbContext.Medics.Local.ToObservableCollection();

            dbContext.Specialties.Load();
            Specialties = dbContext.Specialties.Local.ToObservableCollection();

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
