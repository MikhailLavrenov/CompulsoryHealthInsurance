using CHI.Infrastructure;
using CHI.Models;
using CHI.Models.AppSettings;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CHI.ViewModels
{
    class SrzSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        bool showTextPassword;
        bool showProtectedPassword;


        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }
        public bool ShowTextPassword { get => showTextPassword; set => SetProperty(ref showTextPassword, value); }
        public bool ShowProtectedPassword { get => showProtectedPassword; set => SetProperty(ref showProtectedPassword, value); }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SwitchShowPasswordCommand { get; }


        public SrzSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings;
            MainRegionService = mainRegionService;            

            MainRegionService.Header = "Настройки подключения к СРЗ";
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecuteAsync);
            SwitchShowPasswordCommand = new DelegateCommand(SwitchShowPasswordExecute);
        }

      
        void SetDefaultExecute()
        {
            Settings.Srz.SetDefault();

            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }

        async void TestExecuteAsync()
        {
            MainRegionService.ShowProgressBar("Проверка настроек.");
            await Settings.TestConnectionSRZAsync();

            if (Settings.Srz.ConnectionIsValid)
                MainRegionService.HideProgressBar("Настройки корректны.");
            else if (Settings.Common.ContainsErrorMessage(nameof(Settings.Common.ProxyAddress),ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
            else if (Settings.Srz.ContainsErrorMessage(nameof(Settings.Srz.Address), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Web-сайт СРЗ не доступен.");
            else
                MainRegionService.HideProgressBar($"Не удалось авторизоваться под некоторыми учетными записями.");
        }

        void SwitchShowPasswordExecute()
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
    }
}