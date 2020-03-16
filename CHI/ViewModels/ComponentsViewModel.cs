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

        public DelegateCommand<Type> EditIndicatorsCommand { get; }
        public DelegateCommand AddDetailCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }

        public ComponentsViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;
            mainRegionService.Header = "Показатели";

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();

            root = dbContext.Components.Local.Where(x => x.IsRoot).First();

            RefreshComponents();

            EditIndicatorsCommand = new DelegateCommand<Type>(EditIndicatorsExecute);
            AddDetailCommand = new DelegateCommand(AddDetailExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentComponent?.IsRoot == false).ObservesProperty(() => CurrentComponent);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, () => CurrentComponent?.IsRoot == false).ObservesProperty(() => CurrentComponent);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, () => CurrentComponent?.IsRoot == false).ObservesProperty(() => CurrentComponent);

            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RefreshComponents()
        {
            root.OrderRecursive();

            Components = new ObservableCollection<Component>(root.ToListRecursive());
        }

        private void AddDetailExecute()
        {
            if (CurrentComponent.Details == null)
                CurrentComponent.Details = new List<Component>();

            var newComponent = new Component
            {
                Parent = CurrentComponent,
                Name = "Новый компонент",
                Order = CurrentComponent.Details.Count == 0 ? 0 : CurrentComponent.Details.Last().Order + 1
            };
            var insertIndex = Components.IndexOf(CurrentComponent) + newComponent.Order + 1;
            Components.Insert(insertIndex, newComponent);
            dbContext.Components.Add(newComponent);
        }

        private void EditIndicatorsExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Component), CurrentComponent);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        private void DeleteExecute()
        {
            if (CurrentComponent.IsRoot)
                return;

            var offset = CurrentComponent.Parent.Details.IndexOf(CurrentComponent) + 1;

            for (int i = offset; i < CurrentComponent.Parent.Details.Count; i++)
                CurrentComponent.Parent.Details[i].Order--;

            foreach (var item in CurrentComponent.ToListRecursive())
                Components.Remove(item);

            dbContext.Remove(CurrentComponent);
        }

        private void MoveUpExecute()
        {
            if (CurrentComponent.Parent == null || CurrentComponent.Order == 0)
                return;

            var previous = CurrentComponent.Parent.Details.First(x => x.Order == CurrentComponent.Order - 1);

            CurrentComponent.Order--;
            previous.Order++;

            RefreshComponents();
        }

        private void MoveDownExecute()
        {
            if (CurrentComponent.Parent == null || CurrentComponent.Order == CurrentComponent.Parent.Details.Max(x => x.Order))
                return;

            var next = CurrentComponent.Parent.Details.First(x => x.Order == CurrentComponent.Order + 1);

            CurrentComponent.Order++;
            next.Order--;

            RefreshComponents();
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
