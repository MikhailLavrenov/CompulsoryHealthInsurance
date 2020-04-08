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
    public class SeviceClassifiersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
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
        public DelegateCommand<Type> NavigateCommand { get; }

        public SeviceClassifiersViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;          

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();

            root = dbContext.Components.Local.Where(x => x.IsRoot).First();

            RefreshComponents();

            AddCommand = new DelegateCommand(AddExecute, () => CurrentComponent != null).ObservesProperty(() => CurrentComponent);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentComponent != null && !CurrentComponent.IsRoot).ObservesProperty(() => CurrentComponent);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);

            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void RefreshComponents()
        {
            root.OrderChildsRecursive();

            Components = new ObservableCollection<Component>(root.ToListRecursive());
        }

        private void AddExecute()
        {
            if (CurrentComponent.Childs == null)
                CurrentComponent.Childs = new List<Component>();

            var nextOrder = CurrentComponent.Childs.Count == 0 ? 0 : CurrentComponent.Childs.Last().Order + 1;
            var insertIndex = Components.IndexOf(CurrentComponent) + nextOrder + 1;

            var newComponent = new Component
            {
                Parent = CurrentComponent,
                Name = "Новый компонент",
                Order = nextOrder
            };

            Components.Insert(insertIndex, newComponent);

            CurrentComponent.Childs.Add(newComponent);
        }

        private void DeleteExecute()
        {
            var parentDetails = CurrentComponent.Parent.Childs;
            var offset = parentDetails.IndexOf(CurrentComponent);

            parentDetails.Remove(CurrentComponent);

            for (int i = offset; i < parentDetails.Count; i++)
                parentDetails[i].Order--;

            RefreshComponents();
        }

        private void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Component), CurrentComponent);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            mainRegionService.Header = "Компоненты";

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
