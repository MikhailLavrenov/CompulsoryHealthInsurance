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
        private СredentialScope credentialsScope;
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
        public СredentialScope CredentialsScope { get => credentialsScope; set => SetProperty(ref credentialsScope, value); }
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
        //устанавливает по-умолчанию настройки для файла пациентов
        public void PatiensFileSetDefault()
        {
            PatientsFileFullPath = "Прикрепленные пациенты выгрузка.xlsx";
            FormatPatientsFile = true;
            ColumnProperties = new ObservableCollection<ColumnProperties>()
             {
                    new ColumnProperties{Order=0,   Name="ENP",         AltName="Номер полис",           Hide=false,  Delete=false},
                    new ColumnProperties{Order=1,   Name="FIO",         AltName="ФИО",                   Hide=false,  Delete=false},
                    new ColumnProperties{Order=2,   Name="SEX",         AltName="Пол",                   Hide=false,  Delete=false},
                    new ColumnProperties{Order=3,   Name="BIRTHDAY",    AltName="Дата рождения",         Hide=false,  Delete=false},
                    new ColumnProperties{Order=4,   Name="SNILS",       AltName="СНИЛС",                 Hide=false,  Delete=false},
                    new ColumnProperties{Order=5,   Name="OLR_NUM",     AltName="Участок",               Hide=false,  Delete=false},
                    new ColumnProperties{Order=6,   Name="TERRITORY",   AltName="Регион",                Hide=false,  Delete=false},
                    new ColumnProperties{Order=7,   Name="DISTRICT",    AltName="Район",                 Hide=false,  Delete=false},
                    new ColumnProperties{Order=8,   Name="CITY",        AltName="Город",                 Hide=false,  Delete=false},
                    new ColumnProperties{Order=9,   Name="TOWN",        AltName="Населенный пункт",      Hide=false,  Delete=false},
                    new ColumnProperties{Order=10,  Name="STREET",      AltName="Улица",                 Hide=false,  Delete=false},
                    new ColumnProperties{Order=11,  Name="HOUSE",       AltName="Дом",                   Hide=false,  Delete=false},
                    new ColumnProperties{Order=12,  Name="CORPUS",      AltName="Корпус",                Hide=false,  Delete=false},
                    new ColumnProperties{Order=13,  Name="FLAT",        AltName="Квартира",              Hide=false,  Delete=false},
                    new ColumnProperties{Order=14,  Name="PC_COMM",     AltName="Примечание",            Hide=false,  Delete=false},
                    new ColumnProperties{Order=15,  Name="LAR_NAME",    AltName="Причина прикрепления",  Hide=false,  Delete=false},
                    new ColumnProperties{Order=16,  Name="DOC_SNILS",   AltName="СНИЛС врача",           Hide=false,  Delete=false},
                    new ColumnProperties{Order=17,  Name="SMOCODE",     AltName="Код СМО",               Hide=false,  Delete=false},
                    new ColumnProperties{Order=18,  Name="KLSTREET",    AltName="Код КЛАДР",             Hide=false,  Delete=true},
                    new ColumnProperties{Order=19,  Name="DISTRICTMO",  AltName="Участок МИС",           Hide=false,  Delete=true},
                    new ColumnProperties{Order=20,  Name="PC_BDATE",    AltName="PC_BDATE",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=21,  Name="PERSON_ID",   AltName="ID Пациента",           Hide=false,  Delete=true},
                    new ColumnProperties{Order=22,  Name="PC_ID",       AltName="PC_ID",                 Hide=false,  Delete=true},
                    new ColumnProperties{Order=23,  Name="MO_CODE",     AltName="MO_CODE",               Hide=false,  Delete=true},
                    new ColumnProperties{Order=24,  Name="MO_EDNUM",    AltName="MO_EDNUM",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=25,  Name="PC_NUM",      AltName="PC_NUM",                Hide=false,  Delete=true},
                    new ColumnProperties{Order=26,  Name="PC_EDATE",    AltName="PC_EDATE",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=27,  Name="LAT_CODE",    AltName="LAT_CODE",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=28,  Name="LAT_NAME",    AltName="LAT_NAME",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=29,  Name="LAR_CODE",    AltName="LAR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=30,  Name="LDR_CODE",    AltName="LDR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=31,  Name="LDR_NAME",    AltName="LDR_NAME",              Hide=false,  Delete=true},
                    new ColumnProperties{Order=32,  Name="PC_IDATE",    AltName="PC_IDATE",              Hide=false,  Delete=true},
             };
        }
        //устанавливает по-умолчанию настройки для СРЗ-сайта
        public void SRZSetDefault()
        {
            SiteAddress = @"http://11.0.0.1/";
            UseProxy = false;
            ProxyAddress = "";
            ProxyPort = 0;
            ThreadsLimit = 20;
            CredentialsScope = СredentialScope.ТекущийПользователь;
            Credentials = new ObservableCollection<Credential>()
             {
                    new Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        #endregion
    }

}
