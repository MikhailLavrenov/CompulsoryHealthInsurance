using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class DepartmentsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Department> departments;
        Department currentDepartment;
        Department root;
        List<Department> parents;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }
        public Department CurrentDepartment { get => currentDepartment; set => SetProperty(ref currentDepartment, value); }
        public List<Department> Parents { get => parents; set => SetProperty(ref parents, value); }
        public ObservableCollection<Department> Departments { get => departments; set => SetProperty(ref departments, value); }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }

        public DepartmentsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;
            mainRegionService.Header = "Подразделения";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Departments.Load();

            root = dbContext.Departments.Local.Where(x => x.IsRoot).First();

            RefreshDepartments();

            AddCommand = new DelegateCommand(AddExecute, () => CurrentDepartment != null).ObservesProperty(() => CurrentDepartment);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentDepartment != null && !CurrentDepartment.IsRoot).ObservesProperty(() => CurrentDepartment);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentDepartment);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentDepartment);

            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RefreshDepartments()
        {
            root.OrderChildsRecursive();

            Departments = new ObservableCollection<Department>(root.ToListRecursive());
        }

        private void AddExecute()
        {
            if (CurrentDepartment.Childs == null)
                CurrentDepartment.Childs = new List<Department>();

            var nextOrder = CurrentDepartment.Childs.Count == 0 ? 0 : CurrentDepartment.Childs.Last().Order + 1;
            var insertIndex = Departments.IndexOf(CurrentDepartment) + nextOrder + 1;

            var newDepartment = new Department
            {
                Parent = CurrentDepartment,
                Name = "Новый компонент",
                Order = nextOrder
            };

            Departments.Insert(insertIndex, newDepartment);

            CurrentDepartment.Childs.Add(newDepartment);
        }

        private void DeleteExecute()
        {
            var parentChilds = CurrentDepartment.Parent.Childs;
            var offset = parentChilds.IndexOf(CurrentDepartment);

            parentChilds.Remove(CurrentDepartment);

            for (int i = offset; i < parentChilds.Count; i++)
                parentChilds[i].Order--;

            RefreshDepartments();
        }

        private bool MoveUpCanExecute()
        {
            return CurrentDepartment != null
                && !CurrentDepartment.IsRoot
                && CurrentDepartment.Order != 0;
        }

        private void MoveUpExecute()
        {
            var previous = CurrentDepartment.Parent.Childs.First(x => x.Order == CurrentDepartment.Order - 1);

            CurrentDepartment.Order--;
            previous.Order++;

            RefreshDepartments();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private bool MoveDownCanExecute()
        {
            return CurrentDepartment != null
                && !CurrentDepartment.IsRoot
                && CurrentDepartment != CurrentDepartment.Parent.Childs.Last();
        }

        private void MoveDownExecute()
        {
            var next = CurrentDepartment.Parent.Childs.First(x => x.Order == CurrentDepartment.Order + 1);

            CurrentDepartment.Order++;
            next.Order--;

            RefreshDepartments();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
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
