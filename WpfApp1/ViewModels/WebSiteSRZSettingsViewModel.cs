using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System.Collections.Generic;

namespace PatientsFomsRepository.ViewModels
{
    class WebSiteSRZSettingsViewModel
    {

        public Settings CurrentSettings { get; set; }


        public RelayCommand SaveCommand { get; }
        public void ExecuteSaveCommand(object parameter)
        {
            CurrentSettings.Save();
        }

        public RelayCommand CancelCommand;
        public void ExecuteCancelCommand(object parameter)
        {
            CurrentSettings = Settings.Load();
        }

        public RelayCommand SetDefaultCommand;
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


        public WebSiteSRZSettingsViewModel()
        {
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SetDefaultCommand = new RelayCommand(ExecuteSetDefaultCommand);
        }


    }
}
