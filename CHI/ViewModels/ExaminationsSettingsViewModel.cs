using CHI.Infrastructure;
using CHI.Models.AppSettings;
using Prism.Commands;
using Prism.Regions;

namespace CHI.ViewModels
{
    class ExaminationsSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        private bool showTextPassword;
        private bool showProtectedPassword;


        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public AppSettings Settings { get; set; }
        public bool ShowTextPassword { get => showTextPassword; set => SetProperty(ref showTextPassword, value); }
        public bool ShowProtectedPassword { get => showProtectedPassword; set => SetProperty(ref showProtectedPassword, value); }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SwitchShowPasswordCommand { get; }


        public ExaminationsSettingsViewModel(AppSettings settings, IMainRegionService mainRegionService)
        {
            Settings = settings;
            MainRegionService = mainRegionService;

            MainRegionService.Header = "Настройки загрузки на портал диспансеризации";
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecuteAsync);
            SwitchShowPasswordCommand = new DelegateCommand(SwitchShowPasswordExecute);
        }


        private void SetDefaultExecute()
        {
            Settings.MedicalExaminations.SetDefault();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }

        async void TestExecuteAsync()
        {
            MainRegionService.ShowProgressBar("Проверка настроек.");
            await Settings.TestConnectionExaminationsAsync();

            if (Settings.MedicalExaminations.ConnectionIsValid)
                MainRegionService.HideProgressBar("Настройки корректны.");
            else if (Settings.Common.ContainsErrorMessage(nameof(Settings.Common.ProxyAddress), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
            else if (Settings.MedicalExaminations.ContainsErrorMessage(nameof(Settings.MedicalExaminations.Address), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Портал диспансеризации не доступен.");
            else
                MainRegionService.HideProgressBar($"Не удалось авторизоваться под некоторыми учетными записями.");
        }

        private void SwitchShowPasswordExecute()
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
    }
}
;