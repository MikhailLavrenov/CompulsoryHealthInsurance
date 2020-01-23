using CHI.Application.Infrastructure;
using CHI.Application.Models;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CHI.Application.ViewModels
{
    class SrzSettingsViewModel : DomainObject, IRegionMemberLifetime
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
        public SrzSettingsViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;
            Settings = Settings.Instance;

            //MainRegionService.Header = "Настройки подключения к СРЗ";
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
            Settings.SetDefaultSRZ();

            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
        private void TestExecute()
        {
            MainRegionService.SetBusyStatus("Проверка настроек.");
            Settings.TestConnectionSRZ();

            if (Settings.SrzConnectionIsValid)
                MainRegionService.SetCompleteStatus("Настройки корректны.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.ProxyAddress),ErrorMessages.Connection))
                MainRegionService.SetCompleteStatus("Прокси сервер не доступен.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.SRZAddress), ErrorMessages.Connection))
                MainRegionService.SetCompleteStatus("Web-сайт СРЗ не доступен.");
            else
                MainRegionService.SetCompleteStatus($"Не удалось авторизоваться под некоторыми учетными записями.");
        }
        private void SwitchShowPasswordExecute()
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
        #endregion
    }
}