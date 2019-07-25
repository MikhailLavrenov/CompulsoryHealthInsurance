using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.IO;

namespace PatientsFomsRepository.ViewModels
{
    public class WebSRZSettingsViewModel:BindableBase, IViewModel
    {
        //https://rachel53461.wordpress.com/2011/12/18/navigation-with-mvvm-2/

        #region Fields
        private WebSRZ.Settings currentSettings;
        #endregion

        #region Properties
        public string FullCaption { get; set; }
        public string ShortCaption { get; set; }
        public WebSRZ.Settings CurrentSettings { get => currentSettings; set => SetProperty(ref currentSettings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        #endregion

        #region Creators
        public WebSRZSettingsViewModel()
        {
            FullCaption = "Настройки подключения к web-сайту СРЗ ХК ФОМС";
            ShortCaption = "Настройки подключения";
            SaveCommand = new RelayCommand(ExecuteSave);
            LoadCommand = new RelayCommand(ExecuteLoad, CanExecuteLoad);
            SetDefaultCommand = new RelayCommand(ExecuteSetDefault);
            CurrentSettings = WebSRZ.Settings.Load();
        }
        #endregion

        #region Methods
        public void ExecuteSave(object parameter)
        {
            CurrentSettings.Save();
        }        
        public void ExecuteLoad(object parameter)
        {
            CurrentSettings = Settings.Load();
        }
        public void ExecuteSetDefault(object parameter)
        {
            CurrentSettings.SiteAddress = @"http://11.0.0.1/";
            CurrentSettings.UseProxy = false;
            CurrentSettings.ProxyAddress = "";
            CurrentSettings.ProxyPort = 0;
            CurrentSettings.ThreadsLimit = 20;
            CurrentSettings.EncryptLevel = 0;
            CurrentSettings.Credentials = new List<WebSRZ.Credential>()
             {
                    new WebSRZ.Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new WebSRZ.Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new WebSRZ.Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        public bool CanExecuteLoad(object parameter)
        {
            return File.Exists(Settings.thisFileName);
        }                
        #endregion
    }
}
