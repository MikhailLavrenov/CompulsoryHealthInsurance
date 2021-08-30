using CHI.Infrastructure;
using CHI.Settings;
using Prism.Commands;
using Prism.Regions;

namespace CHI.ViewModels
{
    class SrzSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        bool showPassword;

        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }
        public bool ShowPassword { get => showPassword; set => SetProperty(ref showPassword, value); }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SwitchShowPasswordCommand { get; }


        public SrzSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Настройки подключения к СРЗ";
            ShowPassword = false;

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecuteAsync);
            SwitchShowPasswordCommand = new DelegateCommand(() => ShowPassword = !ShowPassword);
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
            else if (Settings.Common.ContainsErrorMessage(nameof(Settings.Common.ProxyAddress), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
            else if (Settings.Srz.ContainsErrorMessage(nameof(Settings.Srz.Address), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Web-сайт СРЗ не доступен.");
            else
                MainRegionService.HideProgressBar($"Не удалось авторизоваться под некоторыми учетными записями.");
        }

    }
}