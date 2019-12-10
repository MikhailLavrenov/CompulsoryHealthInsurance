using CHI.Application.Infrastructure;
using CHI.Application.Models;
using Prism.Commands;
using Prism.Regions;
using System.Linq;
using System.Text;

namespace CHI.Application.ViewModels
{
    public class ServicesSettingsViewModel : DomainObject, IRegionMemberLifetime
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
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SetDefaultCommand { get; }
        public DelegateCommandAsync TestCommand { get; }
        public DelegateCommand SwitchShowPasswordCommand { get; }
        #endregion

        #region Конструкторы
        public ServicesSettingsViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;
            Settings = Settings.Instance;

            MainRegionService.Header = "Настройки подключения к сервисам ФОМС";
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;

            SaveCommand = new DelegateCommand(SaveExecute);
            LoadCommand = new DelegateCommand(LoadExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecute);
            SwitchShowPasswordCommand = new DelegateCommand(SwitchShowPasswordExecute);
        }
        #endregion

        #region Методы        
        private void SaveExecute()
        {
            Settings.Save();
            MainRegionService.SetCompleteStatus("Настройки сохранены.");
        }
        private void LoadExecute()
        {
            Settings = Settings.Load();
            MainRegionService.SetCompleteStatus("Изменения настроек отменены.");
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultFomsServices();
            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
        private void TestExecute()
        {
            MainRegionService.SetBusyStatus("Проверка настроек.");
            Settings.TestConnection();

            if (Settings.SrzConnectionIsValid && Settings.ExaminationsConnectionIsValid)
                MainRegionService.SetCompleteStatus("Настройки корректны.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.ProxyAddress),ErrorMessages.Connection))
                MainRegionService.SetCompleteStatus("Прокси сервер не доступен.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.SRZAddress), ErrorMessages.Connection))
                MainRegionService.SetCompleteStatus("Web-сайт СРЗ не доступен.");
            else if (Settings.ContainsErrorMessage(nameof(Settings.MedicalExaminationsAddress), ErrorMessages.Connection))
                MainRegionService.SetCompleteStatus("Портал диспансеризации не доступен.");
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
;