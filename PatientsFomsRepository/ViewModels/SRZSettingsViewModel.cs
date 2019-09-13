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
        public IActiveViewModel ActiveViewModel { get; set; }
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
        public SRZSettingsViewModel()
        {
        }
        public SRZSettingsViewModel(IActiveViewModel activeViewModel)
        {
            ActiveViewModel = activeViewModel;
            Settings = Settings.Instance;

            ActiveViewModel.Header = "Настройки подключения к СРЗ ХК ФОМС";
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;
            SaveCommand = new DelegateCommand(SaveCommandExecute);
            LoadCommand = new DelegateCommand(LoadCommandExecute);
            SetDefaultCommand = new DelegateCommand(SetDefaultExecute);
            TestCommand = new DelegateCommandAsync(TestExecute);
            SwitchShowPasswordCommand = new DelegateCommand(SwitchShowPasswordExecute);
        }
        #endregion

        #region Методы        
        private void SaveCommandExecute()
        {
            Settings.Save();
            ActiveViewModel.Status = "Настройки сохранены.";
        }
        private void LoadCommandExecute()
        {
            Settings = Settings.Load();
            ActiveViewModel.Status = "Изменения настроек отменены.";
        }
        private void SetDefaultExecute()
        {
            Settings.SetDefaultSRZ();
            ActiveViewModel.Status = "Настройки установлены по умолчанию.";
        }
        private void TestExecute()
        {
            ActiveViewModel.Status = "Ожидайте. Проверка настроек...";
            Settings.TestConnection();

            if (Settings.ConnectionIsValid)
                ActiveViewModel.Status = "Завершено. Настройки корректны.";
            else if (Settings.ProxyIsNotValid)
                ActiveViewModel.Status = "Завершено. Прокси сервер не доступен.";
            else if (Settings.SiteAddressIsNotValid)
                ActiveViewModel.Status = "Завершено. Web-сайт СРЗ не доступен.";
            else if (Settings.CredentialsIsNotValid)
            {
                var logins = new StringBuilder();
                Settings.Credentials
                .Where(x => x.IsNotValid)
                .ToList()
                .ForEach(x => logins.Append(x).Append(", "));
                logins.Remove(logins.Length - 3, 2);

                ActiveViewModel.Status = $"Завершено. Учетные записи не верны: {logins}. ";
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