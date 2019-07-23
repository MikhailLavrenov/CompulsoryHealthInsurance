using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.IO;

namespace PatientsFomsRepository.ViewModels
{
    class WebSiteSRZSettingsViewModel:BindableBase
    {
        //https://rachel53461.wordpress.com/2011/12/18/navigation-with-mvvm-2/

        #region Fields
        private Settings currentSettings;
        #endregion

        #region Properties
        public Settings CurrentSettings { get => currentSettings; set => SetProperty(ref currentSettings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        #endregion

        #region Creators
        public WebSiteSRZSettingsViewModel()
        {
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SetDefaultCommand = new RelayCommand(ExecuteSetDefaultCommand);
            CurrentSettings = Settings.Load();
        }
        #endregion

        #region Methods
        public void ExecuteSaveCommand(object parameter)
        {
            CurrentSettings.Save();
        }        
        public void ExecuteCancelCommand(object parameter)
        {
            CurrentSettings = Settings.Load();
        }
        public bool CanExecuteCancelCommand(object parameter)
        {
            return File.Exists(Settings.thisFileName);
        }        
        public void ExecuteSetDefaultCommand(object parameter)
        {

            CurrentSettings.SiteAddress = @"http://11.0.0.1/";
            CurrentSettings.UseProxy = false;
            CurrentSettings.ProxyAddress = "";
            CurrentSettings.ProxyPort = 0;
            CurrentSettings.ThreadsLimit = 20;
            CurrentSettings.EncryptLevel = 0;
            CurrentSettings.Credentials = new List<Credential>()
             {
                    new FomsPatientsDB.Models.Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new FomsPatientsDB.Models.Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new FomsPatientsDB.Models.Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        #endregion
    }
}
