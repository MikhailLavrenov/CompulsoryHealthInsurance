using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.IO;

namespace PatientsFomsRepository.ViewModels
{
    public class SRZSettingsViewModel:BindableBase, IViewModel
    {
        //https://rachel53461.wordpress.com/2011/12/18/navigation-with-mvvm-2/

        #region Fields
        private Settings currentSettings;
        #endregion

        #region Properties
        public string ViewModelHeader { get; set; }
        public Settings CurrentSettings { get => currentSettings; set => SetProperty(ref currentSettings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        #endregion

        #region Creators
        public SRZSettingsViewModel()
        {
            ViewModelHeader = "Настройки подключения к web-сайту СРЗ ХК ФОМС";
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel, CanExecuteCancel);
            SetDefaultCommand = new RelayCommand(ExecuteSetDefault);
            CurrentSettings = Settings.Load();
        }
        #endregion

        #region Methods
        public void ExecuteSave(object parameter)
        {
            CurrentSettings.Save();
        }        
        public void ExecuteCancel(object parameter)
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
            CurrentSettings.Credentials = new List<Credential>()
             {
                    new Models.Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new Models.Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new Models.Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        public bool CanExecuteCancel(object parameter)
        {
            return File.Exists(Settings.thisFileName);
        }                
        #endregion
    }
}
