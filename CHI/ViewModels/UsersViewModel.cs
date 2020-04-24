using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services.WindowsAccounts;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class UsersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<User> serviceClassifiers;
        User currentUser;
        IMainRegionService mainRegionService;

        public bool KeepAlive { get; set; }
        public User CurrentUser { get => currentUser; set => SetProperty(ref currentUser, value); }
        public ObservableCollection<User> Users { get => serviceClassifiers; set => SetProperty(ref serviceClassifiers, value); }

        public DelegateCommand<Type> AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }

        public UsersViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            dbContext = new ServiceAccountingDBContext();

            dbContext.Users.Load();

            Users = dbContext.Users.Local.ToObservableCollection();

            AddCommand = new DelegateCommand<Type>(AddExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute, () => CurrentUser != null).ObservesProperty(() => CurrentUser);
            NavigateCommand = new DelegateCommand<Type>(NavigateExecute);
        }

        private void AddExecute(Type view)
        {
            var newUser = new User();

            Users.Add(newUser);

            CurrentUser = newUser;

            NavigateExecute(view);
        }

        private void DeleteExecute()
        {
            Users.Remove(CurrentUser);
        }

        private void NavigateExecute(Type view)
        {
            KeepAlive = true;

            var navigationParameters = new NavigationParameters();
            navigationParameters.Add(nameof(User), CurrentUser);
            mainRegionService.RequestNavigate(view.Name, navigationParameters, true);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            mainRegionService.Header = "Пользователи";

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
