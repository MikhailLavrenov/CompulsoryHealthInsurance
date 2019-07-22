using FomsPatientsDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    class Settings
        {

        public string SiteAddress { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string PatientsFileFullPath { get; set; }
        public int ThreadsLimit { get; set; }
        public Credential[] Credentials { get; set; }
        public int EncryptLevel { get; set; }


        public bool FormatPatientsFile { get; set; }
        public PatientsFile.ColumnAttribute[] ColumnAttributes { get; set; }

        [XmlIgnoreAttribute] public bool TestPassed { get; set; }

        }
    }
