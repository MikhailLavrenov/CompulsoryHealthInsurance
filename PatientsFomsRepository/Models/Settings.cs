using PatientsFomsRepository.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    public class Settings : BindableBase
        {
        #region Поля
        private string siteAddress;
        private bool useProxy;
        private string proxyAddress;
        private int proxyPort;        
        private int threadsLimit;
        private int encryptLevel;
        private ObservableCollection<Credential> credentials;
        private bool testPassed;
        private string patientsFileFullPath;
        private bool formatPatientsFile;        
        private ObservableCollection<ColumnProperties> columnProperties;
        #endregion

        #region Свойства        
        public static string ThisFileName { get; } = "Settings.xml";
        public string SiteAddress { get => siteAddress; set => SetProperty(ref siteAddress, value); }
        public bool UseProxy { get => useProxy; set => SetProperty(ref useProxy, value); }
        public string ProxyAddress { get => proxyAddress; set => SetProperty(ref proxyAddress, value); }
        public int ProxyPort { get => proxyPort; set => SetProperty(ref proxyPort, value); }        
        public int ThreadsLimit { get => threadsLimit; set => SetProperty(ref threadsLimit, value); }
        public int EncryptLevel { get => encryptLevel; set => SetProperty(ref encryptLevel, value); }
        public ObservableCollection<Credential> Credentials { get => credentials; set => SetProperty(ref credentials, value); }
        [XmlIgnoreAttribute] public bool TestPassed { get => testPassed; set => SetProperty(ref testPassed, value); }
        public string PatientsFileFullPath { get => patientsFileFullPath; set => SetProperty(ref patientsFileFullPath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnProperties> ColumnProperties { get => columnProperties; set => SetProperty(ref columnProperties, value); }
        #endregion

        #region Конструкторы
        public Settings()
        {
            Credentials = new ObservableCollection<Credential>();
            ColumnProperties = new ObservableCollection<ColumnProperties>();
        }
        #endregion

        #region Методы
        //сохраняет настройки в xml
        public void Save()
            {
            TestPassed = false;
            using (var stream = new FileStream(ThisFileName, FileMode.Create))
                {
                var formatter = new XmlSerializer(GetType());
                formatter.Serialize(stream, this);
                }
            }
        //загружает настройки из xml
        public static Settings Load()
            {
            if (File.Exists(ThisFileName))
                using (var stream = new FileStream(ThisFileName, FileMode.Open))
                    {
                    var formatter = new XmlSerializer(typeof(Settings));
                    return formatter.Deserialize(stream) as Settings;
                    }
            else
                return new Settings();
            }
        #endregion
        }
    }
