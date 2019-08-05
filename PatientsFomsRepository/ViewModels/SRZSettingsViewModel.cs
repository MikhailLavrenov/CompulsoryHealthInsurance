using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace PatientsFomsRepository.ViewModels
{
    public class SRZSettingsViewModel : BindableBase, IViewModel
    {
        

        #region Поля
        private Settings settings;
        private bool showTextPassword;
        private bool showProtectedPassword;
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public bool ShowTextPassword { get => showTextPassword; set => SetProperty(ref showTextPassword, value); }
        public bool ShowProtectedPassword { get => showProtectedPassword; set => SetProperty(ref showProtectedPassword, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand TestCommand { get; }
        public RelayCommand SwitchShowPasswordCommand { get; }
        #endregion

        #region Конструкторы
        public SRZSettingsViewModel()
        {
            ShortCaption = "Настройки подключения к СРЗ";
            FullCaption = "Настройки подключения к web-сайту СРЗ ХК ФОМС";
            SaveCommand = new RelayCommand(x=> Settings.Save());
            LoadCommand = new RelayCommand(x=> Settings = Settings.Load());
            SetDefaultCommand = new RelayCommand(x=> Settings.SRZSetDefault());
            TestCommand = new RelayCommand(x=> Settings.TestConnection());
            SwitchShowPasswordCommand = new RelayCommand(ExecuteSwitchShowPassword);

            Settings = Settings.Load();
            ShowTextPassword = false;
            ShowProtectedPassword = !ShowTextPassword;
        }
        #endregion

        #region Методы
        private void ExecuteSwitchShowPassword(object parameter)
        {
            ShowTextPassword = !ShowTextPassword;
            ShowProtectedPassword = !ShowTextPassword;
        }
        #endregion
    }
}
;