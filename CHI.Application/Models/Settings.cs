using CHI.Application.Infrastructure;
using CHI.Services.MedicalExaminations;
using CHI.Services.SRZ;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CHI.Application.Models
{
    public class Settings : DomainObject
    {
        #region Поля
        //SRZ
        private readonly int timeoutConnection = 3000;
        private string srzAddress;
        private string medicalExaminationsAddress;
        private bool useProxy;
        private string proxyAddress;
        private ushort proxyPort;
        private byte threadsLimit;
        private CredentialScope credentialsScope;
        private ObservableCollection<Credential> credentials;
        private bool srzConnectionIsValid;     

        //PatientsFile
        private bool downloadNewPatientsFile;
        private string patientsFilePath;
        private bool formatPatientsFile;
        private ObservableCollection<ColumnProperty> columnProperties;

        //MedicalExaminations
        private string examinationFileNames;
        private string patientFileNames;
        private bool examinationsConnectionIsValid;
        #endregion

        #region Свойства       
        public static string SettingsFileName { get; }
        public static Settings Instance { get; private set; }

        //SRZ
        public string SRZAddress { get => srzAddress; set => SetProperty(ref srzAddress, FixUrl(value)); }
        [XmlIgnore] public bool SrzConnectionIsValid { get => srzConnectionIsValid; set => SetProperty(ref srzConnectionIsValid, value); }

        //Services 
        public bool UseProxy
        {
            get => useProxy;
            set
            {
                SetProperty(ref useProxy, value);
                if (value == false)
                {
                    ProxyAddress = "";
                    ProxyPort = 0;
                }

            }
        }
        public string ProxyAddress
        {
            get => proxyAddress;
            set
            {
                value = value.Trim();
                SetProperty(ref proxyAddress, value);
            }
        }
        public ushort ProxyPort
        {
            get => proxyPort;
            set => SetProperty(ref proxyPort, value);
        }
        public byte ThreadsLimit { get => threadsLimit; set => SetProperty(ref threadsLimit, value); }
        public CredentialScope CredentialsScope { get => credentialsScope; set { Credential.Scope = value; SetProperty(ref credentialsScope, value); } }
        public ObservableCollection<Credential> Credentials { get => credentials; set => SetProperty(ref credentials, value); }               

        //PatientsFile
        public bool DownloadNewPatientsFile { get => downloadNewPatientsFile; set => SetProperty(ref downloadNewPatientsFile, value); }
        public string PatientsFilePath { get => patientsFilePath; set => SetProperty(ref patientsFilePath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnProperty> ColumnProperties { get => columnProperties; set => SetProperty(ref columnProperties, value); }

        //MedicalExaminations
        public string MedicalExaminationsAddress { get => medicalExaminationsAddress; set => SetProperty(ref medicalExaminationsAddress, FixUrl(value)); }
        public string ExaminationFileNames { get => examinationFileNames; set => SetProperty(ref examinationFileNames, value); }
        public string PatientFileNames { get => patientFileNames; set => SetProperty(ref patientFileNames, value); }
        [XmlIgnore] public bool ExaminationsConnectionIsValid { get => examinationsConnectionIsValid; set => SetProperty(ref examinationsConnectionIsValid, value); }
        #endregion

        #region Конструкторы
        static Settings()
        {
            SettingsFileName = "Settings.xml";
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
            RemoveError(ErrorMessages.Connection, nameof(SRZAddress));
            Credentials.ToList().ForEach(x => x.RemoveErrorsMessage(ErrorMessages.Connection));

            if (UseProxy && !TryConnectProxy())
            {
                AddError(ErrorMessages.Connection, nameof(ProxyAddress));
                AddError(ErrorMessages.Connection, nameof(ProxyPort));
                SrzConnectionIsValid = false;
                ExaminationsConnectionIsValid = false;
                return;
            }
            else
            {
                RemoveError(ErrorMessages.Connection, nameof(ProxyAddress));
                RemoveError(ErrorMessages.Connection, nameof(ProxyPort));
            }

            var connectedSrz = TryConnectSite(SRZAddress);
            if (!connectedSrz)
            {
                AddError(ErrorMessages.Connection, nameof(SRZAddress));
                SrzConnectionIsValid = false;
            }

            var connectedMedicaExaminations = TryConnectSite(MedicalExaminationsAddress);
            if (!connectedMedicaExaminations)
            {
                AddError(ErrorMessages.Connection, nameof(MedicalExaminationsAddress));
                ExaminationsConnectionIsValid = false;
            }

            if (!connectedSrz && !connectedMedicaExaminations)
                return;

            if (connectedSrz && !TryAuthorizeCredentialsInSrzService())
            {
                SrzConnectionIsValid = false;
                ExaminationsConnectionIsValid = false;
                return;
            }
            else if (connectedMedicaExaminations && !TryAuthorizeCredentialsInExaminationsService())
            {
                SrzConnectionIsValid = false;
                ExaminationsConnectionIsValid = false;
                return;
            }

            SrzConnectionIsValid = connectedSrz && true;
            ExaminationsConnectionIsValid = connectedMedicaExaminations && true;
        }
        //сохраняет настройки в xml
        public void Save()
        {
            SrzConnectionIsValid = false;
            ExaminationsConnectionIsValid = false;

            // Т.к. при создании экзмпляра класса если свойства не инициализируются - не срабатывает валидация. Поэтому принудительно проверяем все. 
            // Свойства не инициализируются сразу т.к. иначе сразу после создания на них будут отображаться ошибки, и во View тоже, это плохо.
            foreach (var item in Credentials)
                item.Validate();

            foreach (var item in ColumnProperties)
                item.Validate();

            using (var stream = new FileStream(SettingsFileName, FileMode.Create))
            {
                var formatter = new XmlSerializer(GetType());
                formatter.Serialize(stream, this);
            }
        }
        //загружает настройки из xml
        public static Settings Load()
        {
            if (File.Exists(SettingsFileName))
                using (var stream = new FileStream(SettingsFileName, FileMode.Open))
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
        public void SetDefaultFomsServices()
        {
            SRZAddress = @"http://11.0.0.1/";
            MedicalExaminationsAddress = @"http://11.0.0.2/";
            UseProxy = false;
            ProxyAddress = "";
            ProxyPort = 0;
            ThreadsLimit = 5;
            CredentialsScope = CredentialScope.ТекущийПользователь;
            Credentials = new ObservableCollection<Credential>()
             {
                    new Credential{Login="МойЛогин1", Password="МойПароль1", RequestsLimit=400},
                    new Credential{Login="МойЛогин2", Password="МойПароль2", RequestsLimit=300},
                    new Credential{Login="МойЛогин3", Password="МойПароль3", RequestsLimit=500}
             };
        }
        //Устанавливает значения по умолчанию для портала диспансеризации
        public void SetDefaultMedicalExaminations()
        {
            PatientFileNames = @"LPM, LVM, LOM";
            ExaminationFileNames = @"DPM, DVM, DOM";
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
        //Валидация свойств
        public override void Validate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SRZAddress):
                    if (string.IsNullOrEmpty(SRZAddress) || Uri.TryCreate(SRZAddress, UriKind.Absolute, out _) == false)
                        AddError(ErrorMessages.UriFormat, propertyName);
                    else
                        RemoveError(ErrorMessages.UriFormat, propertyName);
                    break;

                case nameof(MedicalExaminationsAddress):
                    if (string.IsNullOrEmpty(MedicalExaminationsAddress) || Uri.TryCreate(MedicalExaminationsAddress, UriKind.Absolute, out _) == false)
                        AddError(ErrorMessages.UriFormat, propertyName);
                    else
                        RemoveError(ErrorMessages.UriFormat, propertyName);
                    break;

                case nameof(UseProxy):
                    if (UseProxy == false)
                    {
                        RemoveErrors(nameof(ProxyAddress));
                        RemoveErrors(nameof(ProxyPort));
                    }
                    break;

                case nameof(ProxyAddress):
                    if (UseProxy)
                        ValidateIsNullOrEmptyString(nameof(ProxyAddress), ProxyAddress);
                    break;

                case nameof(PatientsFilePath):
                    ValidateIsNullOrEmptyString(nameof(PatientsFilePath), PatientsFilePath);
                    break;

                case nameof(ThreadsLimit):
                    if (ThreadsLimit < 1)
                        AddError(ErrorMessages.LessOne, propertyName);
                    else
                        RemoveError(ErrorMessages.LessOne, propertyName);
                    break;
            }
        }
        //проверяет настройки прокси-сервера, в случае успеха - true
        private bool TryConnectProxy()
        {
            //if (!UseProxy)
            //    return true;

            var connected = false;

            using (var client = new TcpClient())
            {
                try
                {
                    connected = client.ConnectAsync(ProxyAddress, ProxyPort).Wait(timeoutConnection);
                }
                catch (Exception)
                { }
            }
            return connected;
        }
        //проверяет доступность сайта, в случае успеха - true
        private bool TryConnectSite(string url)
        {
            var connected = false;

            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Timeout = timeoutConnection;

                if (UseProxy)
                    webRequest.Proxy = new WebProxy(ProxyAddress + ":" + ProxyPort);

                webRequest.GetResponse();
                webRequest.Abort();

                connected = true;
            }
            catch (Exception)
            { }

            return connected;
        }
        //проверяет учетные данные, в случае успеха - true
        private bool TryAuthorizeCredentialsInSrzService()
        {
            Parallel.ForEach(Credentials, credential =>
            {
                using (var service = new SRZService(SRZAddress, UseProxy, ProxyAddress, ProxyPort))
                {
                    if (service.Authorize(credential))
                    {
                        service.Logout();

                        credential.RemoveErrors(nameof(credential.Login));
                        credential.RemoveErrors(nameof(credential.Password));
                    }
                    else
                    {
                        credential.AddError(ErrorMessages.Connection, nameof(credential.Login));
                        credential.AddError(ErrorMessages.Connection, nameof(credential.Password));
                    }
                }

            });

            return !Credentials.Any(x => x.HasErrors);
        }
        private bool TryAuthorizeCredentialsInExaminationsService()
        {
            Parallel.ForEach(Credentials, credential =>
            {
                using (var service = new ExaminationService(SRZAddress, UseProxy, ProxyAddress, ProxyPort))
                {
                    if (service.Authorize(credential))
                    {
                        service.Logout();

                        credential.RemoveErrors(nameof(credential.Login));
                        credential.RemoveErrors(nameof(credential.Password));
                    }
                    else
                    {
                        credential.AddError(ErrorMessages.Connection, nameof(credential.Login));
                        credential.AddError(ErrorMessages.Connection, nameof(credential.Password));
                    }
                }

            });

            return !Credentials.Any(x => x.HasErrors);
        }
        private static string FixUrl(string url)
        {
            url = url.Trim().ToLower();

            if (!string.IsNullOrEmpty(url))
            {
                if (!url.StartsWith(@"http://") && !url.StartsWith(@"https://"))
                    url = $@"http://{url}";

                if (url.EndsWith(@"/") == false)
                    url = $@"{url}/";
            }

            return url;
        }
        #endregion
    }
}
