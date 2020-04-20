using CHI.Infrastructure;
using CHI.Models;
using Prism.Commands;
using Prism.Regions;
using System.Linq;
using System.Text;

namespace CHI.ViewModels
{
    class ExaminationsSettingsViewModel : DomainObject, IRegionMemberLifetime
    {
        #region Поля
        private Settings settings;
        private bool showTextPassword;
        private bool showProtectedPassword;
        #endregion

        #region Свойства
        public IMainRegionService MainRegionService { get; set; }
        public bool KeepAlive { get => false; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public bool ShowTextPassword { get => showTextPassword; set => SetProperty(ref showTextPassword, value); }
        public bool ShowProtectedPassword { get => showProtectedPassword; set => SetProperty(ref showProtectedPassword, value); }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SwitchShowPasswordCommand { get; }
        #endregion

        #region Конструкторы
        public ExaminationsSettingsViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;
            Settings = Settings.Instance;

            MainRegionService.Header = "Настройки загрузки периодических осмотров";
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;

            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecute);
            SwitchShowPasswordCommand = new DelegateCommand(SwitchShowPasswordExecute);
        }
        #endregion

        #region Методы        
        private void SetDefaultExecute()
        {
            Settings.SetDefaultExaminations();
            MainRegionService.HideProgressBar("Настройки установлены по умолчанию.");
        }
        private void TestExecute()
        {
            MainRegionService.ShowProgressBar("Проверка настроек.");
            Settings.TestConnectionExaminations();

            if (Settings.ExaminationsConnectionIsValid)
                MainRegionService.HideProgressBar("Настройки корректны.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.ProxyAddress),ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Прокси сервер не доступен.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.ExaminationsAddress), ErrorMessages.Connection))
                MainRegionService.HideProgressBar("Портал диспансеризации не доступен.");
            else
                MainRegionService.HideProgressBar($"Не удалось авторизоваться под некоторыми учетными записями.");
        }
        private void SwitchShowPasswordExecute()
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
        #endregion
    }
}
;