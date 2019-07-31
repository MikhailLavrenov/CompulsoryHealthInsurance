using PatientsFomsRepository.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
    {
    public class Settings : BindableBase
        {
        #region Поля
        //SRZ
        private string siteAddress;
        private bool useProxy;
        private string proxyAddress;
        private ushort proxyPort;
        private byte threadsLimit;
        private CredentialScope credentialsScope;
        private ObservableCollection<Credential> credentials;

        //валидация SRZ
        private bool siteAddressIsNotValid;
        private bool proxyIsNotValid;
        private bool credentialsIsNotValid;
        private bool connectionIsValid;

        //PatientsFile
        private bool downloadNewPAtientsFile;
        private string patientsFilePath;
        private bool formatPatientsFile;
        private ObservableCollection<ColumnProperty> columnsProperty;
        #endregion

        #region Свойства        
        public static string ThisFileName { get; } = "Settings.xml";

        //SRZ
        public string SiteAddress { get => siteAddress; set => SetProperty(ref siteAddress, value); }
        public bool UseProxy { get => useProxy; set => SetProperty(ref useProxy, value); }
        public string ProxyAddress { get => proxyAddress; set => SetProperty(ref proxyAddress, value); }
        public ushort ProxyPort { get => proxyPort; set => SetProperty(ref proxyPort, value); }
        public byte ThreadsLimit { get => threadsLimit; set => SetProperty(ref threadsLimit, value); }
        public CredentialScope CredentialsScope { get => credentialsScope; set { Credential.Scope = value; SetProperty(ref credentialsScope, value); } }
        public ObservableCollection<Credential> Credentials { get => credentials; set => SetProperty(ref credentials, value); }

        //валидация SRZ
        [XmlIgnore] public bool SiteAddressIsNotValid { get => siteAddressIsNotValid; set => SetProperty(ref siteAddressIsNotValid, value); }
        [XmlIgnore] public bool ProxyIsNotValid { get => proxyIsNotValid; set => SetProperty(ref proxyIsNotValid, value); }
        [XmlIgnore] public bool CredentialsIsNotValid { get => credentialsIsNotValid; set => SetProperty(ref credentialsIsNotValid, value); }
        [XmlIgnore] public bool ConnectionIsValid { get => connectionIsValid; set => SetProperty(ref connectionIsValid, value); }

        //PatientsFile
        public bool DownloadNewPatientsFile { get => downloadNewPAtientsFile; set => SetProperty(ref downloadNewPAtientsFile, value); }
        public string PatientsFilePath { get => patientsFilePath; set => SetProperty(ref patientsFilePath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnProperty> ColumnsProperty { get => columnsProperty; set => SetProperty(ref columnsProperty, value); }
        #endregion

        #region Конструкторы
        public Settings()
            {
            Credentials = new ObservableCollection<Credential>();
            ColumnsProperty = new ObservableCollection<ColumnProperty>();
            }
        #endregion

        #region Методы
        //проверяет настройки прокси-сервера
        private void TestProxy()
            {
            if (UseProxy)
                {
                var client = new TcpClient();
                var connected = client.ConnectAsync(ProxyAddress, ProxyPort).Wait(10000);
                ProxyIsNotValid = !connected;
                client.Close();
                }
            else
                ProxyIsNotValid = false;
            }
        //проверяет доступность сайта
        private void TestSite()
            {
            if (ProxyIsNotValid == false)
                {
                try
                    {
                    var webRequest = (HttpWebRequest)WebRequest.Create(SiteAddress);
                    webRequest.Timeout = 10000;
                    if (useProxy)
                        webRequest.Proxy = new WebProxy(ProxyAddress + ":" + ProxyPort);

                    webRequest.GetResponse();
                    webRequest.Abort();
                    }
                catch (Exception)
                    {
                    SiteAddressIsNotValid = true;
                    return;
                    }
                }

            SiteAddressIsNotValid = false;
            }
        //проверяет учетные данные
        private void TestCredentials()
            {
            if (SiteAddressIsNotValid == false)
                {
                Parallel.ForEach(Credentials, credential =>
                {
                    using (SRZ site = new SRZ(SiteAddress, ProxyAddress, ProxyPort))
                        {
                        credential.IsNotValid = site.TryAuthorize(credential);
                        }
                });
                CredentialsIsNotValid = Credentials.FirstOrDefault(x => x.IsNotValid == true) != null;
                }
            else
                CredentialsIsNotValid = false;
            }
        //проверить настройки
        public void TestConnection()
            {
            TestProxy();
            TestSite();
            TestCredentials();
            ConnectionIsValid = SiteAddressIsNotValid && ProxyIsNotValid && CredentialsIsNotValid && false;
            }
        //сохраняет настройки в xml
        public void Save()
            {
            ConnectionIsValid = false;
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
            DownloadNewPatientsFile = true;
            PatientsFilePath = "Прикрепленные пациенты выгрузка.xlsx";
            FormatPatientsFile = true;
            ColumnsProperty = new ObservableCollection<ColumnProperty>()
             {
                    new ColumnProperty{Name="ENP",         AltName="Номер полис",           Hide=false,  Delete=false},
                    new ColumnProperty{Name="FIO",         AltName="ФИО",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="SEX",         AltName="Пол",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="BIRTHDAY",    AltName="Дата рождения",         Hide=false,  Delete=false},
                    new ColumnProperty{Name="SNILS",       AltName="СНИЛС",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="OLR_NUM",     AltName="Участок",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="TERRITORY",   AltName="Регион",                Hide=false,  Delete=false},
                    new ColumnProperty{Name="DISTRICT",    AltName="Район",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="CITY",        AltName="Город",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="TOWN",        AltName="Населенный пункт",      Hide=false,  Delete=false},
                    new ColumnProperty{Name="STREET",      AltName="Улица",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="HOUSE",       AltName="Дом",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="CORPUS",      AltName="Корпус",                Hide=false,  Delete=false},
                    new ColumnProperty{Name="FLAT",        AltName="Квартира",              Hide=false,  Delete=false},
                    new ColumnProperty{Name="PC_COMM",     AltName="Примечание",            Hide=false,  Delete=false},
                    new ColumnProperty{Name="LAR_NAME",    AltName="Причина прикрепления",  Hide=false,  Delete=false},
                    new ColumnProperty{Name="DOC_SNILS",   AltName="СНИЛС врача",           Hide=false,  Delete=false},
                    new ColumnProperty{Name="SMOCODE",     AltName="Код СМО",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="KLSTREET",    AltName="Код КЛАДР",             Hide=false,  Delete=true},
                    new ColumnProperty{Name="DISTRICTMO",  AltName="Участок МИС",           Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_BDATE",    AltName="PC_BDATE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PERSON_ID",   AltName="ID Пациента",           Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_ID",       AltName="PC_ID",                 Hide=false,  Delete=true},
                    new ColumnProperty{Name="MO_CODE",     AltName="MO_CODE",               Hide=false,  Delete=true},
                    new ColumnProperty{Name="MO_EDNUM",    AltName="MO_EDNUM",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_NUM",      AltName="PC_NUM",                Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_EDATE",    AltName="PC_EDATE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAT_CODE",    AltName="LAT_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAT_NAME",    AltName="LAT_NAME",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LAR_CODE",    AltName="LAR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LDR_CODE",    AltName="LDR_CODE",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="LDR_NAME",    AltName="LDR_NAME",              Hide=false,  Delete=true},
                    new ColumnProperty{Name="PC_IDATE",    AltName="PC_IDATE",              Hide=false,  Delete=true},
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
            CredentialsScope = CredentialScope.ТекущийПользователь;
            Credentials = new ObservableCollection<Credential>()
             {
                    new Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
            }
        //сдвигает вверх элемент коллекции ColumnsProperty
        public void MoveUp(ColumnProperty item)
            {
            var itemIndex = ColumnsProperty.IndexOf(item);
            if (itemIndex > 0)
                {
                var previousItem = ColumnsProperty[itemIndex - 1];
                ColumnsProperty[itemIndex - 1] = item;
                ColumnsProperty[itemIndex] = previousItem;
                }
            }
        //сдвигает вниз элемент коллекции ColumnsProperty
        public void MoveDown(ColumnProperty item)
            {
            var itemIndex = ColumnsProperty.IndexOf(item);
            if (itemIndex != -1 && itemIndex < ColumnsProperty.Count - 1)
                {
                var nextItem = ColumnsProperty[itemIndex + 1];
                ColumnsProperty[itemIndex + 1] = item;
                ColumnsProperty[itemIndex] = nextItem;
                }
            }
        #endregion
        }
    }
