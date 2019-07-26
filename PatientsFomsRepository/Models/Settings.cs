using PatientsFomsRepository.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    public class Settings : BindableBase
        {
        #region Fields
        private string siteAddress;
        private bool useProxy;
        private string proxyAddress;
        private int proxyPort;
        private string patientsFileFullPath;
        private int threadsLimit;
        private int encryptLevel;
        private bool formatPatientsFile;
        #endregion

        #region Properties        
        public static string ThisFileName { get; } = "Settings.xml";
        public string SiteAddress { get => siteAddress; set => SetProperty(ref siteAddress, value); }
        public bool UseProxy { get => useProxy; set => SetProperty(ref useProxy, value); }
        public string ProxyAddress { get => proxyAddress; set => SetProperty(ref proxyAddress, value); }
        public int ProxyPort { get => proxyPort; set => SetProperty(ref proxyPort, value); }        
        public int ThreadsLimit { get => threadsLimit; set => SetProperty(ref threadsLimit, value); }
        public int EncryptLevel { get => encryptLevel; set => SetProperty(ref encryptLevel, value); }
        public ObservableCollection<Credential> Credentials { get; set; }
        [XmlIgnoreAttribute] public bool TestPassed { get; set; }
        public string PatientsFileFullPath { get => patientsFileFullPath; set => SetProperty(ref patientsFileFullPath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnAttribute> ColumnAttributes { get; set; }        
        #endregion

        #region Methods
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
