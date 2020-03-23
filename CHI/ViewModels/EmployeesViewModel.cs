using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class EmployeesViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        Employee currentEmployee;
        ObservableCollection<Employee> employees;

        public bool KeepAlive { get => false; }
        public Employee CurrentEmployee
        {
            get => currentEmployee;
            set
            {
                if (currentEmployee != null)
                    currentEmployee.PropertyChanged -= CurrentEmployee_PropertyChanged;

                SetProperty(ref currentEmployee, value);

                if (currentEmployee != null)
                    currentEmployee.PropertyChanged += CurrentEmployee_PropertyChanged;
            }
        }
        public ObservableCollection<Employee> Employees { get => employees; set => SetProperty(ref employees, value); }
        public List<Medic> Medics { get; set; }
        public List<Specialty> Specialties { get; set; }
        public List<Department> Departments { get; set; }

        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        public EmployeesViewModel(IMainRegionService mainRegionService)
        {
            mainRegionService.Header = "Штатные единицы";

            dbContext = new ServiceAccountingDBContext();

            dbContext.Employees.Load();
            Medics = dbContext.Medics.ToList();
            Specialties = dbContext.Specialties.ToList();
            Departments = dbContext.Departments.Where(x=>x.IsRoot || x.Childs==null).ToList();           

            RefreshCommand = new DelegateCommand(RefreshExecute);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentEmployee);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentEmployee);

            RefreshExecute();
        }

        private bool MoveUpCanExecute()
        {
            if (CurrentEmployee == null || Employees.First() == CurrentEmployee)
                return false;

            var previuosEmployee = Employees[Employees.IndexOf(CurrentEmployee) - 1];

            return CurrentEmployee.Department == previuosEmployee.Department;
        }

        private void MoveUpExecute()
        {
            var itemIndex = Employees.IndexOf(CurrentEmployee);

            var previousEmployee = Employees[itemIndex - 1];

            previousEmployee.Order++;
            CurrentEmployee.Order--;

            Employees.Move(itemIndex, itemIndex - 1);

            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }

        private bool MoveDownCanExecute()
        {
            if (CurrentEmployee == null || Employees.Last() == CurrentEmployee)
                return false;

            var nextEmployee = Employees[Employees.IndexOf(CurrentEmployee) + 1];

            return CurrentEmployee.Department == nextEmployee.Department;
        }

        private void MoveDownExecute()
        {
            var itemIndex = Employees.IndexOf(CurrentEmployee);

            var nextEmployee = Employees[itemIndex + 1];

            CurrentEmployee.Order++;
            nextEmployee.Order--;

            Employees.Move(itemIndex, itemIndex + 1);

            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }

        private void RefreshExecute()
        {
            foreach (var department in Departments.Where(x => x.Employees != null && x.Employees.Count > 0))
            {               
                department.Employees = department.Employees.OrderBy(x => x.Order).ToList();

                for (int i = 0; i < department.Employees.Count; i++)
                    if (department.Employees[i].Order != i)
                        department.Employees[i].Order = i;
            }

            var sortedEmployees = dbContext.Employees.Local.OrderBy(x => x.Order).OrderBy(x => x.Department.Name).ToList();

            Employees = new ObservableCollection<Employee>(sortedEmployees);
        }

        private void CurrentEmployee_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentEmployee.Department))
            {
                dbContext.ChangeTracker.CascadeChanges();

                CurrentEmployee.Order = CurrentEmployee.Department.Employees.Count;
                RefreshExecute();
            }
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
