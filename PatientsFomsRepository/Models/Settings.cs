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
        private readonly int timeoutConnection = 5000;
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
        private bool downloadNewPatientsFile;
        private string patientsFilePath;
        private bool formatPatientsFile;
        private ObservableCollection<ColumnProperty> columnProperties;
        #endregion

        #region Свойства       
        [XmlIgnore] public static string ThisFileName { get; } = "Settings.xml";
        public static Settings Instance { get; private set; }

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
        public bool DownloadNewPatientsFile { get => downloadNewPatientsFile; set => SetProperty(ref downloadNewPatientsFile, value); }
        public string PatientsFilePath { get => patientsFilePath; set => SetProperty(ref patientsFilePath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnProperty> ColumnProperties { get => columnProperties; set => SetProperty(ref columnProperties, value); }
        #endregion

        #region Конструкторы
        static Settings()
        {
            Instance = Load();
        }
        public Settings()
        {
            Credentials = new ObservableCollection<Credential>();
            ColumnProperties = new ObservableCollection<ColumnProperty>();
            Instance = this;
        }
        #endregion

        #region Методы
        //проверить настройки
        public void TestConnection()
        {
            TestProxy();
            TestSite();
            TestCredentials();
            ConnectionIsValid = SiteAddressIsNotValid == false && ProxyIsNotValid == false && CredentialsIsNotValid == false;
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
        public void SetDefaultPatiensFile()
        {
            DownloadNewPatientsFile = true;
            PatientsFilePath = "Прикрепленные пациенты выгрузка.xlsx";
            FormatPatientsFile = true;
            ColumnProperties = new ObservableCollection<ColumnProperty>()
             {
                    new ColumnProperty{Name="ENP",         AltName="Полис",                 Hide=false,  Delete=false},
                    new ColumnProperty{Name="FIO",         AltName="ФИО",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="Фамилия",     AltName="Фамилия",               Hide=false,  Delete=false},
                    new ColumnProperty{Name="Имя",         AltName="Имя",                   Hide=false,  Delete=false},
                    new ColumnProperty{Name="Отчество",    AltName="Отчество",              Hide=false,  Delete=false},
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
                    new ColumnProperty{Name="PRV_TYPE",    AltName="PRV_TYPE",              Hide=false,  Delete=true},
             };
        }
        //устанавливает по-умолчанию настройки для СРЗ-сайта
        public void SetDefaultSRZ()
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
        //сдвигает вверх элемент коллекции ColumnProperties
        public void MoveUpColumnProperty(ColumnProperty item)
        {
            var itemIndex = ColumnProperties.IndexOf(item);
            if (itemIndex > 0)
                ColumnProperties.Move(itemIndex, itemIndex - 1);
        }
        //сдвигает вниз элемент коллекции ColumnProperties
        public void MoveDownColumnProperty(ColumnProperty item)
        {
            var itemIndex = ColumnProperties.IndexOf(item);
            if (itemIndex >= 0 && itemIndex < ColumnProperties.Count - 1)
                ColumnProperties.Move(itemIndex, itemIndex + 1);
        }
        //проверяет настройки прокси-сервера
        private void TestProxy()
        {
            if (UseProxy)
            {
                var client = new TcpClient();

                try
                {
                    var connected = client.ConnectAsync(ProxyAddress, ProxyPort).Wait(timeoutConnection);
                    ProxyIsNotValid = !connected;
                }
                catch (Exception)
                {
                    ProxyIsNotValid = true;
                }
                finally
                {
                    client.Close();
                }
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
                    webRequest.Timeout = timeoutConnection;
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
                        credential.IsNotValid = site.TryAuthorize(credential) == false;
                    }
                });
                CredentialsIsNotValid = Credentials.FirstOrDefault(x => x.IsNotValid == true) != null;
            }
            else
                CredentialsIsNotValid = false;
        }
        #endregion
    }
}
