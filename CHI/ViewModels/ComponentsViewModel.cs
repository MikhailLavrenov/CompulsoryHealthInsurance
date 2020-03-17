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
    public class ComponentsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<Component> components;
        Component currentComponent;
        Component root;
        List<Component> parents;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public List<Component> Parents { get => parents; set => SetProperty(ref parents, value); }
        public ObservableCollection<Component> Components { get => components; set => SetProperty(ref components, value); }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<Type> EditIndicatorsCommand { get; }

        public ComponentsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;
            mainRegionService.Header = "Показатели";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();

            root = dbContext.Components.Local.Where(x => x.IsRoot).First();

            RefreshComponents();

            AddCommand = new DelegateCommand(AddExecute, () => CurrentComponent != null).ObservesProperty(() => CurrentComponent);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentComponent != null && !CurrentComponent.IsRoot).ObservesProperty(() => CurrentComponent);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentComponent);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentComponent);
            EditIndicatorsCommand = new DelegateCommand<Type>(EditIndicatorsExecute);

            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RefreshComponents()
        {
            root.OrderRecursive();

            Components = new ObservableCollection<Component>(root.ToListRecursive());
        }

        private void AddExecute()
        {
            if (CurrentComponent.Details == null)
                CurrentComponent.Details = new List<Component>();

            var nextOrder = CurrentComponent.Details.Count == 0 ? 0 : CurrentComponent.Details.Last().Order + 1;
            var insertIndex = Components.IndexOf(CurrentComponent) + nextOrder + 1;

            var newComponent = new Component
            {
                Parent = CurrentComponent,
                Name = "Новый компонент",
                Order = nextOrder
            };

            Components.Insert(insertIndex, newComponent);

            CurrentComponent.Details.Add(newComponent);
        }

        private void DeleteExecute()
        {
            var parentDetails = CurrentComponent.Parent.Details;
            var offset = parentDetails.IndexOf(CurrentComponent);

            parentDetails.Remove(CurrentComponent);

            for (int i = offset; i < parentDetails.Count; i++)
                parentDetails[i].Order--;

            RefreshComponents();
        }

        private bool MoveUpCanExecute()
        {
            return CurrentComponent != null
                && !CurrentComponent.IsRoot
                && CurrentComponent.Order != 0;
        }

        private void MoveUpExecute()
        {
            var previous = CurrentComponent.Parent.Details.First(x => x.Order == CurrentComponent.Order - 1);

            CurrentComponent.Order--;
            previous.Order++;

            RefreshComponents();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private bool MoveDownCanExecute()
        {
            return CurrentComponent != null
                && !CurrentComponent.IsRoot
                && CurrentComponent != CurrentComponent.Parent.Details.Last();
        }

        private void MoveDownExecute()
        {
            var next = CurrentComponent.Parent.Details.First(x => x.Order == CurrentComponent.Order + 1);

            CurrentComponent.Order++;
            next.Order--;

            RefreshComponents();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private void EditIndicatorsExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Component), CurrentComponent);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            KeepAlive = false;
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
