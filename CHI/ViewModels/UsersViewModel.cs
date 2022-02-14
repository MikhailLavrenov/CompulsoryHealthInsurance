using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using CHI.Services;
using CHI.Settings;
using DryIoc;
using Microsoft.EntityFrameworkCore;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CHI.ViewModels
{
    public class UsersViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        AppDBContext dbContext;
        ObservableCollection<User> serviceClassifiers;
        User currentUser;
        private readonly AppSettings settings;
        IMainRegionService mainRegionService;
        User currentAppUser;
        IContainer container;

        public bool KeepAlive { get; set; }
        public User CurrentUser { get => currentUser; set => SetProperty(ref currentUser, value); }
        public ObservableCollection<User> Users { get => serviceClassifiers; set => SetProperty(ref serviceClassifiers, value); }

        public DelegateCommand<Type> AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand<Type> NavigateCommand { get; }

        public UsersViewModel(AppSettings settings, IMainRegionService mainRegionService, User currentUser, IContainer container)
        {
            this.settings = settings;
            this.mainRegionService = mainRegionService;
            this.container = container;
            currentAppUser = currentUser;

            dbContext = new AppDBContext(settings.Common.SqlServer, settings.Common.SqlDatabase, settings.Common.SqlLogin, settings.Common.SqlPassword);

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

            var usr = dbContext.Users.Where(x => x.Sid == currentAppUser.Sid).Include(x => x.PlanningPermisions).FirstOrDefault();

            if (usr != null)
                container.UseInstance(usr);
        }
    }
}
