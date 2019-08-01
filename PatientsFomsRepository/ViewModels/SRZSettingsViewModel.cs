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
        #endregion

        #region Свойства
        public string ShortCaption { get; set; }
        public string FullCaption { get; set; }
        public Settings Settings { get => settings; set => SetProperty(ref settings, value); }
        public RelayCommand SaveCommand { get; }
        public RelayCommand LoadCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand TestCommand { get; }
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
            Settings = Settings.Load();
        }
        #endregion

        #region Методы
        #endregion
    }
}
;