using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public Settings CurrentSettings { get => currentSettings; set => SetProperty(ref currentSettings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand DefaultCommand { get; }
        public RelayCommand TestCommand { get; }
        #endregion

        #region Creators
        public SRZSettingsViewModel()
        {
            ShortCaption = "Настройки подключения к СРЗ";
            FullCaption = "Настройки подключения к web-сайту СРЗ ХК ФОМС";
            SaveCommand = new RelayCommand(ExecuteSave);
            LoadCommand = new RelayCommand(ExecuteLoad);
            DefaultCommand = new RelayCommand(ExecuteDefault);
            TestCommand = new RelayCommand(ExecuteTest);
            CurrentSettings = Settings.Load();
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
        public void ExecuteDefault(object parameter)
        {
            CurrentSettings.SiteAddress = @"http://11.0.0.1/";
            CurrentSettings.UseProxy = false;
            CurrentSettings.ProxyAddress = "";
            CurrentSettings.ProxyPort = 0;
            CurrentSettings.ThreadsLimit = 20;
            CurrentSettings.EncryptLevel = 0;            
            CurrentSettings.Credentials = new ObservableCollection<Credential>()
             {
                    new Models.Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new Models.Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new Models.Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        public void ExecuteTest(object parameter)
        {
        }

        #endregion
    }
}
;