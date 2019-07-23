using FomsPatientsDB.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    class Settings : BaseModel
        {
        //private static readonly Settings instance = Load();
        public static string thisFileName { get; } = "Settings.xml";

        private string siteAddress;
        public string SiteAddress { get => siteAddress; set => SetField(ref siteAddress, value); }

        private bool useProxy;
        public bool UseProxy { get => useProxy; set => SetField(ref useProxy, value); }

        private string proxyAddress;
        public string ProxyAddress { get => proxyAddress; set => SetField(ref proxyAddress, value); }

        private int proxyPort;
        public int ProxyPort { get => proxyPort; set => SetField(ref proxyPort, value); }

        private string patientsFileFullPath;
        public string PatientsFileFullPath { get => patientsFileFullPath; set => SetField(ref patientsFileFullPath, value); }

        private int threadsLimit;
        public int ThreadsLimit { get => threadsLimit; set => SetField(ref threadsLimit, value); }

        private int encryptLevel;
        public int EncryptLevel { get => encryptLevel; set => SetField(ref encryptLevel, value); }

        private List<Credential> credentials;
        public List<Credential> Credentials { get => credentials; set => SetField(ref credentials, value); }

        private bool formatPatientsFile;
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetField(ref formatPatientsFile, value); }

        private List<PatientsFile.ColumnAttribute> columnAttributes;
        public List<PatientsFile.ColumnAttribute> ColumnAttributes { get => columnAttributes; set => SetField(ref columnAttributes, value); }

        [XmlIgnoreAttribute] public bool TestPassed { get; set; }



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
        }
    }
