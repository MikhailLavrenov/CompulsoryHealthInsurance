using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
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
        IDialogService dialogService;

        public bool KeepAlive { get; set; }
        public Component CurrentComponent { get => currentComponent; set => SetProperty(ref currentComponent, value); }
        public List<Component> Parents { get => parents; set => SetProperty(ref parents, value); }
        public ObservableCollection<Component> Components { get => components; set => SetProperty(ref components, value); }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }
        public DelegateCommand SelectColorCommand { get; }

        public ComponentsViewModel(IMainRegionService mainRegionService, IDialogService dialogService)
        {
            this.mainRegionService = mainRegionService;
            this.dialogService = dialogService;

            dbContext = new ServiceAccountingDBContext();
            dbContext.Components.Load();

            root = dbContext.Components.Local.Where(x => x.IsRoot).First();

            Refresh();

            AddCommand = new DelegateCommand(AddExecute, () => CurrentComponent != null).ObservesProperty(() => CurrentComponent);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentComponent != null && !CurrentComponent.IsRoot).ObservesProperty(() => CurrentComponent);
            MoveUpCommand = new DelegateCommand(MoveUpExecute, MoveUpCanExecute).ObservesProperty(() => CurrentComponent);
            MoveDownCommand = new DelegateCommand(MoveDownExecute, MoveDownCanExecute).ObservesProperty(() => CurrentComponent);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);
            SelectColorCommand = new DelegateCommand(SelectColorExecute);

            DeleteCommand.RaiseCanExecuteChanged();
        }

        private void Refresh()
        {
            root.OrderChildsRecursive();

            Components = new ObservableCollection<Component>(root.ToListRecursive());
        }

        private void AddExecute()
        {
            if (CurrentComponent.Childs == null)
                CurrentComponent.Childs = new List<Component>();

            var nextOrder = CurrentComponent.Childs.Count == 0 ? 0 : CurrentComponent.Childs.Last().Order + 1;
            //var insertIndex = Components.IndexOf(CurrentComponent) + nextOrder + 1;
            //var insertIndex = CurrentComponent.IsRoot? Components.Count : CurrentComponent.Parent.Childs.IndexOf(CurrentComponent)+1

            var newComponent = new Component
            {
                Parent = CurrentComponent,
                Name = "Новый компонент",
                Order = nextOrder
            };

            //Components.Insert(insertIndex, newComponent);

            CurrentComponent.Childs.Add(newComponent);

            Refresh();
        }

        private void DeleteExecute()
        {
            var parentDetails = CurrentComponent.Parent.Childs;
            var offset = parentDetails.IndexOf(CurrentComponent);

            parentDetails.Remove(CurrentComponent);

            for (int i = offset; i < parentDetails.Count; i++)
                parentDetails[i].Order--;

            Refresh();
        }

        private bool MoveUpCanExecute()
        {
            return CurrentComponent != null
                && !CurrentComponent.IsRoot
                && CurrentComponent.Order != 0;
        }

        private void MoveUpExecute()
        {
            var previous = CurrentComponent.Parent.Childs.First(x => x.Order == CurrentComponent.Order - 1);

            CurrentComponent.Order--;
            previous.Order++;

            Refresh();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private bool MoveDownCanExecute()
        {
            return CurrentComponent != null
                && !CurrentComponent.IsRoot
                && CurrentComponent != CurrentComponent.Parent.Childs.Last();
        }

        private void MoveDownExecute()
        {
            var next = CurrentComponent.Parent.Childs.First(x => x.Order == CurrentComponent.Order + 1);

            CurrentComponent.Order++;
            next.Order--;

            Refresh();

            MoveDownCommand.RaiseCanExecuteChanged();
            MoveUpCommand.RaiseCanExecuteChanged();
        }

        private void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(Component), CurrentComponent);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        private void SelectColorExecute()
        {
            CurrentComponent.HexColor = Helpers.ShowColorDialog(dialogService, "Выбор цвета", CurrentComponent.HexColor);
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
