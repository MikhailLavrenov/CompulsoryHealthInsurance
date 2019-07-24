using FomsPatientsDB.Models;
using PatientsFomsRepository.Infrastructure;
using System.Collections.Generic;
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
        private List<Credential> credentials;
        private bool formatPatientsFile;
        private List<PatientsFile.ColumnAttribute> columnAttributes;
        #endregion

        #region Properties        
        public static string thisFileName { get; } = "Settings.xml";
        public string SiteAddress { get => siteAddress; set => SetProperty(ref siteAddress, value); }
        public bool UseProxy { get => useProxy; set => SetProperty(ref useProxy, value); }
        public string ProxyAddress { get => proxyAddress; set => SetProperty(ref proxyAddress, value); }
        public int ProxyPort { get => proxyPort; set => SetProperty(ref proxyPort, value); }
        public string PatientsFileFullPath { get => patientsFileFullPath; set => SetProperty(ref patientsFileFullPath, value); }
        public int ThreadsLimit { get => threadsLimit; set => SetProperty(ref threadsLimit, value); }
        public int EncryptLevel { get => encryptLevel; set => SetProperty(ref encryptLevel, value); }
        public List<Credential> Credentials { get => credentials; set => SetProperty(ref credentials, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public List<PatientsFile.ColumnAttribute> ColumnAttributes { get => columnAttributes; set => SetProperty(ref columnAttributes, value); }
        [XmlIgnoreAttribute] public bool TestPassed { get; set; }
        #endregion

        #region Methods
        //сохраняет настройки в xml
        public void Save()
        {
            TestPassed = false;
            using (var stream = new FileStream(thisFileName, FileMode.Create))
            {
                var formatter = new XmlSerializer(GetType());
                formatter.Serialize(stream, this);
            }
        }
        //загружает настройки из xml
        public static Settings Load()
        {
            using (var stream = new FileStream(thisFileName, FileMode.Open))
            {
                var formatter = new XmlSerializer(typeof(Settings));
                return formatter.Deserialize(stream) as Settings;
            }
        }
        #endregion
    }
}
