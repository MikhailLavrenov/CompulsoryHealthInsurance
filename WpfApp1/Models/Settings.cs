using FomsPatientsDB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    class Settings
        {
        //private static readonly Settings instance = Load();
        public static string thisFileName { get; } = "Settings.xml";



        public string SiteAddress { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string PatientsFileFullPath { get; set; }
        public int ThreadsLimit { get; set; }
        public int EncryptLevel { get; set; }
        public List<Credential> Credentials { get; set; }
        public bool FormatPatientsFile { get; set; }
        public PatientsFile.ColumnAttribute[] ColumnAttributes { get; set; }

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
