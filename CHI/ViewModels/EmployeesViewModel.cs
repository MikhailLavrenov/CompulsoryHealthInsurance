using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class EmployeesViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        Employee currentEmployee;
        ObservableCollection<Employee> employees;
        IMainRegionService mainRegionService;

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
        public List<Department> Departments { get; set; }

        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommandAsync SplitAgesCommand { get; }
        public DelegateCommandAsync MergeAgesCommand { get; }
        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand CleanCommand { get; }

        public EmployeesViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Штатные единицы";

            dbContext = new AppDBContext(settings.Common.SqlServer, settings.Common.SqlDatabase, settings.Common.SqlLogin, settings.Common.SqlPassword);

            dbContext.Employees.Include(x => x.Medic).Include(x => x.Specialty).Load();
            Departments = dbContext.Departments.Where(x => x.IsRoot || x.Childs == null || x.Childs.Count == 0).OrderBy(x => x.Order).ToList();

            RefreshCommand = new DelegateCommand(RefreshExecute);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentEmployee);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentEmployee);
            SplitAgesCommand = new DelegateCommandAsync(SplitAgesExecute, () => CurrentEmployee?.AgeKind == AgeKind.Any).ObservesProperty(() => CurrentEmployee);
            MergeAgesCommand = new DelegateCommandAsync(MergeAgesExecute, () => CurrentEmployee != null && CurrentEmployee.AgeKind != AgeKind.Any).ObservesProperty(() => CurrentEmployee);
            CleanCommand = new DelegateCommand(CleanExecute);

            RefreshExecute();
        }

        bool MoveUpCanExecute()
        {
            if (CurrentEmployee == null || Employees.First() == CurrentEmployee)
                return false;

            var previuosEmployee = Employees[Employees.IndexOf(CurrentEmployee) - 1];

            return CurrentEmployee.Department == previuosEmployee.Department;
        }

        void MoveUpExecute()
        {
            var itemIndex = Employees.IndexOf(CurrentEmployee);

            var previousEmployee = Employees[itemIndex - 1];

            previousEmployee.Order++;
            CurrentEmployee.Order--;

            Employees.Move(itemIndex, itemIndex - 1);

            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }

        bool MoveDownCanExecute()
        {
            if (CurrentEmployee == null || Employees.Last() == CurrentEmployee)
                return false;

            var nextEmployee = Employees[Employees.IndexOf(CurrentEmployee) + 1];

            return CurrentEmployee.Department == nextEmployee.Department;
        }

        void MoveDownExecute()
        {
            var itemIndex = Employees.IndexOf(CurrentEmployee);

            var nextEmployee = Employees[itemIndex + 1];

            CurrentEmployee.Order++;
            nextEmployee.Order--;

            Employees.Move(itemIndex, itemIndex + 1);

            MoveUpCommand.RaiseCanExecuteChanged();
            MoveDownCommand.RaiseCanExecuteChanged();
        }

        void RefreshExecute()
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

        void SplitAgesExecute()
        {
            mainRegionService.ShowProgressBar("Разделение штатной единицы на детские и взрослые");

            dbContext.Parameters.Where(x => x.Employee.Id == CurrentEmployee.Id).Load();

            var adultEmployee = (Employee)CurrentEmployee.Clone();
            adultEmployee.Parameters.ForEach(x => x.Employee = adultEmployee);

            adultEmployee.AgeKind = AgeKind.Adults;
            CurrentEmployee.AgeKind = AgeKind.Сhildren;

            dbContext.Add(adultEmployee);

            var cases = dbContext.Cases
                .Include(x => x.Services).ThenInclude(x => x.Employee)
                .Where(x => x.Employee.Id == CurrentEmployee.Id && x.AgeKind == AgeKind.Adults)
                .ToList();

            foreach (var mcase in cases)
            {
                mcase.Employee = adultEmployee;

                mcase.Services.Where(x => x.Employee.Id == CurrentEmployee.Id).ToList().ForEach(x => x.Employee = adultEmployee);
            }

            dbContext.SaveChanges();

            dbContext.Cases
                .Where(x => x.AgeKind == AgeKind.Adults && x.Services.Any(y => y.Employee.Id == CurrentEmployee.Id))
                .SelectMany(x => x.Services)
                .Where(x => x.Employee.Id == CurrentEmployee.Id)
                .ToList()
                .ForEach(x => x.Employee = adultEmployee);

            dbContext.SaveChanges();

            RefreshExecute();

            MergeAgesCommand.RaiseCanExecuteChanged();


            mainRegionService.HideProgressBar("Разделение штатной единицы на детские и взрослые успешно завершено");
        }

        void MergeAgesExecute()
        {
            mainRegionService.ShowProgressBar("Объединение штатных единиц в одну");

            var removeEmployee = Employees.FirstOrDefault(x => x != CurrentEmployee && x.Medic == CurrentEmployee.Medic && x.Specialty == CurrentEmployee.Specialty && x.AgeKind != AgeKind.Any);

            if (removeEmployee == null)
                throw new InvalidOperationException("Не найден 2ая соответствующая штатная единица");

            dbContext.Cases.Where(x => x.Employee.Id == removeEmployee.Id).ToList().ForEach(x => x.Employee = CurrentEmployee);
            dbContext.Services.Where(x => x.Employee.Id == removeEmployee.Id).ToList().ForEach(x => x.Employee = CurrentEmployee);

            var parametersRemove = dbContext.Parameters.Where(x => x.Employee.Id == removeEmployee.Id).ToList();
            var parametersSkip = dbContext.Parameters.Where(x => x.Employee.Id == CurrentEmployee.Id).ToList();
            var parametersIdsRemove = parametersRemove.Select(x => x.Id).ToList();

            var plans = dbContext.Plans.Where(x => parametersIdsRemove.Contains(x.Parameter.Id)).ToList();

            foreach (var plan in plans)
                plan.Parameter = parametersSkip.First(x => x.Kind == plan.Parameter.Kind);

            dbContext.Remove(removeEmployee);

            CurrentEmployee.AgeKind = AgeKind.Any;

            dbContext.SaveChanges();

            RefreshExecute();

            SplitAgesCommand.RaiseCanExecuteChanged();

            mainRegionService.HideProgressBar("Объединение штатных единиц в одну успешно завершено");
        }

        void CleanExecute()
        {

            mainRegionService.ShowProgressBar("Удаление не связанных с реестрами штатных единиц, мед. работников и специальностей");

            var usedEmployees = dbContext.Cases.Select(x => x.Employee).Distinct().Union(dbContext.Services.Select(x => x.Employee).Distinct()).Distinct();
            var usedEmployeeIds = new HashSet<int>(usedEmployees.Select(x => x.Id));
            var employeesToRemove = dbContext.Employees.Local.Where(x => !usedEmployeeIds.Contains(x.Id)).ToList();

            dbContext.Employees.RemoveRange(employeesToRemove);

            var usedMedics = dbContext.Employees.Local.Select(x => x.Medic).ToList();
            var usedMedicsIds = new HashSet<int>(usedMedics.Select(x => x.Id));
            var medicsToRemove = dbContext.Medics.Local.Where(x => !usedMedicsIds.Contains(x.Id)).ToArray();

            dbContext.Medics.RemoveRange(medicsToRemove);

            var usedSpecialties = dbContext.Employees.Local.Select(x => x.Specialty).ToList();
            var usedSpecialtiesIds = new HashSet<int>(usedSpecialties.Select(x => x.Id));
            var specialtiesToRemove = dbContext.Specialties.Local.Where(x => !usedSpecialtiesIds.Contains(x.Id)).ToArray();

            dbContext.Specialties.RemoveRange(specialtiesToRemove);
            dbContext.SaveChanges();

            RefreshExecute();

            mainRegionService.HideProgressBar("Удаление не связанных с реестрами штатных единиц, мед. работников и специальностей успешно завершено");
        }

        void CurrentEmployee_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
