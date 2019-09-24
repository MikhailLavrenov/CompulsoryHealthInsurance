using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using Prism.Commands;
using Prism.Regions;
using System.Linq;
using System.Text;

namespace PatientsFomsRepository.ViewModels
{
    public class SRZSettingsViewModel : DomainObject, IRegionMemberLifetime
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
        public SRZSettingsViewModel(IMainRegionService mainRegionService)
        {
            MainRegionService = mainRegionService;
            Settings = Settings.Instance;

            MainRegionService.Header = "Настройки подключения к СРЗ ХК ФОМС";
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
            MainRegionService.SetCompleteStatus( "Настройки сохранены.");
        }
        private void LoadExecute()
        {
            Settings = Settings.Load();
            MainRegionService.SetCompleteStatus("Изменения настроек отменены.");
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultSRZ();
            MainRegionService.SetCompleteStatus("Настройки установлены по умолчанию.");
        }
        private void TestExecute()
        {
            MainRegionService.SetBusyStatus( "Проверка настроек.");
            Settings.TestConnection();

            if (Settings.ConnectionIsValid)
                MainRegionService.SetCompleteStatus("Настройки корректны.");
            else if (Settings.ProxyIsNotValid)
                MainRegionService.SetCompleteStatus(" Прокси сервер не доступен.");
            else if (Settings.SiteAddressIsNotValid)
                MainRegionService.SetCompleteStatus("Web-сайт СРЗ не доступен.");
            else if (Settings.CredentialsIsNotValid)
            {
                var logins = new StringBuilder();
                Settings.Credentials
                .Where(x => x.IsNotValid)
                .ToList()
                .ForEach(x => logins.Append(x).Append(", "));
                logins.Remove(logins.Length - 3, 2);

                MainRegionService.SetCompleteStatus($" Учетные записи не верны: {logins}.");
            }
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