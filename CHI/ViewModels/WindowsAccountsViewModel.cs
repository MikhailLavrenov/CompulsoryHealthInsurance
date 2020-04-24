using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.ServiceAccounting;
using CHI.Services.WindowsAccounts;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace CHI.ViewModels
{
    public class WindowsAccountsViewModel : DomainObject, IRegionMemberLifetime, INavigationAware
    {
        ServiceAccountingDBContext dbContext;
        ObservableCollection<WindowsAccount> windowsAccount;
        User currentUser;
        WindowsAccount currentWindowsAccount;
        bool isDomain;
        IMainRegionService mainRegionService;
        WindowsAccountsService accountService;

        public bool KeepAlive { get => false; }
        public bool IsDomain { get => isDomain; set => SetProperty(ref isDomain, value, OnIsDomainChanged); }
        public WindowsAccount CurrentWindowsAccount { get => currentWindowsAccount; set => SetProperty(ref currentWindowsAccount, value); }
        public User CurrentUser { get => currentUser; set => SetProperty(ref currentUser, value); }
        public ObservableCollection<WindowsAccount> WindowsAccounts { get => windowsAccount; set => SetProperty(ref windowsAccount, value); }

        public DelegateCommand OkCommand { get; }


        public WindowsAccountsViewModel(IMainRegionService mainRegionService, IFileDialogService fileDialogService)
        {
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Учетные записи Windows";

            dbContext = new ServiceAccountingDBContext();

            var settings = Settings.Instance;

            accountService = new WindowsAccountsService(settings.DomainName, settings.DomainUsersRootOU);

            IsDomain = accountService.IsDomainAvailable;

            if (!IsDomain)
                OnIsDomainChanged();

            OkCommand = new DelegateCommand(OkExecute, () => CurrentWindowsAccount != null).ObservesProperty(() => CurrentWindowsAccount);
        }


        private void OkExecute()
        {
            CurrentUser.Sid = CurrentWindowsAccount.Sid;
            CurrentUser.Name = CurrentWindowsAccount.Name;

            mainRegionService.RequestNavigateBack();
        }

        private void OnIsDomainChanged()
        {
            var accounts = isDomain ? accountService.Domain : accountService.Local;
            WindowsAccounts = new ObservableCollection<WindowsAccount>(accounts);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(nameof(User)))
                CurrentUser = navigationContext.Parameters.GetValue<User>(nameof(User));
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
