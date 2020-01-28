using CHI.Application.Infrastructure;
using CHI.Services.MedicalExaminations;
using CHI.Services.SRZ;
using System;
using System.Collections.Concurrent;
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
        #region Общие
        private static readonly int timeoutConnection = 3000;
        private static readonly string settingsFileName = "Settings.xml";

        public static Settings Instance { get; private set; }

        static Settings()
        {
            Instance = Load();
        }
        public Settings()
        {
            SrzCredentials = new ObservableCollection<Credential>();
            ExaminationsCredentials = new ObservableCollection<Credential>();
            ColumnProperties = new ObservableCollection<ColumnProperty>();
            Instance = this;
        }

        //сохраняет настройки в xml
        public void Save()
        {
            ProxyConnectionIsValid = false;
            SrzConnectionIsValid = false;
            ExaminationsConnectionIsValid = false;

            // Т.к. при создании экзмпляра класса если свойства не инициализируются - не срабатывает валидация. Поэтому принудительно проверяем все. 
            // Свойства не инициализируются сразу т.к. иначе сразу после создания на них будут отображаться ошибки, и во View тоже, это плохо.
            foreach (var item in SrzCredentials)
                item.Validate();

            foreach (var item in ExaminationsCredentials)
                item.Validate();

            foreach (var item in ColumnProperties)
                item.Validate();

            using (var stream = new FileStream(settingsFileName, FileMode.Create))
            {
                var formatter = new XmlSerializer(GetType());
                formatter.Serialize(stream, this);
            }
        }
        //загружает настройки из xml
        public static Settings Load()
        {
            if (File.Exists(settingsFileName))
                using (var stream = new FileStream(settingsFileName, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(Settings));
                    return formatter.Deserialize(stream) as Settings;
                }
            else
            {
                var settings= new Settings();
                settings.SetDefaultSRZ();
                settings.SetDefaultAttachedPatientsFile();
                settings.SetDefaultExaminations();
                settings.SetDefaultOther();

                return settings;
            }
        }
        //исправляет url
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
        //Валидация свойств
        public override void Validate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(SrzAddress):
                    if (string.IsNullOrEmpty(SrzAddress) || Uri.TryCreate(SrzAddress, UriKind.Absolute, out _) == false)
                        AddError(ErrorMessages.UriFormat, propertyName);
                    else
                        RemoveError(ErrorMessages.UriFormat, propertyName);
                    break;

                case nameof(ExaminationsAddress):
                    if (string.IsNullOrEmpty(ExaminationsAddress) || Uri.TryCreate(ExaminationsAddress, UriKind.Absolute, out _) == false)
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

                case nameof(SrzThreadsLimit):
                    if (SrzThreadsLimit < 1)
                        AddError(ErrorMessages.LessOne, propertyName);
                    else
                        RemoveError(ErrorMessages.LessOne, propertyName);
                    break;

                case nameof(ExaminationsThreadsLimit):
                    if (ExaminationsThreadsLimit < 1)
                        AddError(ErrorMessages.LessOne, propertyName);
                    else
                        RemoveError(ErrorMessages.LessOne, propertyName);
                    break;
            }
        }
        //проверяет доступность сайта, в случае успеха - true
        private bool TryConnectSite(string url, string nameOfAddress)
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Timeout = timeoutConnection;

                if (UseProxy)
                    webRequest.Proxy = new WebProxy(ProxyAddress + ":" + ProxyPort);

                webRequest.GetResponse();
                webRequest.Abort();

                RemoveError(ErrorMessages.Connection, nameOfAddress);
                return true;
            }
            catch (Exception)
            {
                AddError(ErrorMessages.Connection, nameOfAddress);
                return false;
            }

        }
        #endregion

        #region Прочие
        private bool useProxy;
        private string proxyAddress;
        private ushort proxyPort;
        private bool proxyConnectionIsValid;
        private CredentialScope credentialsScope;

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
        [XmlIgnore] public bool ProxyConnectionIsValid { get => proxyConnectionIsValid; set => SetProperty(ref proxyConnectionIsValid, value); }
        public CredentialScope CredentialsScope { get => credentialsScope; set { Credential.Scope = value; SetProperty(ref credentialsScope, value); } }

        //проверяет настройки прокси-сервера, true-соединение установлено или прокси-сервер не используется, false-иначе
        public void TestConnectionProxy()
        {
            var connected = false;

            if (UseProxy)
                using (var client = new TcpClient())
                {
                    try
                    {
                        client.ConnectAsync(ProxyAddress, ProxyPort).Wait(timeoutConnection);
                        connected = client.Connected;
                    }
                    catch (Exception)
                    { }
                }

            if (UseProxy && !connected)
            {
                AddError(ErrorMessages.Connection, nameof(ProxyAddress));
                AddError(ErrorMessages.Connection, nameof(ProxyPort));
                SrzConnectionIsValid = false;
                ExaminationsConnectionIsValid = false;

                ProxyConnectionIsValid = false;
            }
            else
            {
                RemoveError(ErrorMessages.Connection, nameof(ProxyAddress));
                RemoveError(ErrorMessages.Connection, nameof(ProxyPort));

                ProxyConnectionIsValid = true;
            }
        }
        //устанавливает по-умолчанию настройки для прочих настроек
        public void SetDefaultOther()
        {
            UseProxy = false;
            ProxyAddress = "";
            ProxyPort = 0;
            CredentialsScope = CredentialScope.ТекущийПользователь;
        }
        #endregion

        #region Файл прикрепленных пациентов
        private string srzAddress;
        private byte srzThreadsLimit;
        private uint srzRequestsLimit;
        private ObservableCollection<Credential> srzCredentials;
        private bool srzConnectionIsValid;
        private bool downloadNewPatientsFile;
        private string patientsFilePath;
        private bool formatPatientsFile;
        private ObservableCollection<ColumnProperty> columnProperties;

        public string SrzAddress { get => srzAddress; set => SetProperty(ref srzAddress, FixUrl(value)); }
        public byte SrzThreadsLimit { get => srzThreadsLimit; set => SetProperty(ref srzThreadsLimit, value); }
        public uint SrzRequestsLimit { get => srzRequestsLimit; set => SetProperty(ref srzRequestsLimit, value); }
        public ObservableCollection<Credential> SrzCredentials { get => srzCredentials; set => SetProperty(ref srzCredentials, value); }
        [XmlIgnore] public bool SrzConnectionIsValid { get => srzConnectionIsValid; set => SetProperty(ref srzConnectionIsValid, value); }
        public bool DownloadNewPatientsFile { get => downloadNewPatientsFile; set => SetProperty(ref downloadNewPatientsFile, value); }
        public string PatientsFilePath { get => patientsFilePath; set => SetProperty(ref patientsFilePath, value); }
        public bool FormatPatientsFile { get => formatPatientsFile; set => SetProperty(ref formatPatientsFile, value); }
        public ObservableCollection<ColumnProperty> ColumnProperties { get => columnProperties; set => SetProperty(ref columnProperties, value); }

                //проверяет учетные данные СРЗ, в случае успеха - true
        private bool TryAuthorizeSrzCredentials()
        {
            Parallel.ForEach(SrzCredentials, new ParallelOptions { MaxDegreeOfParallelism = SrzThreadsLimit }, credential =>
            {
                using (var service = new SRZService(SrzAddress, UseProxy, ProxyAddress, ProxyPort))
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

            return !SrzCredentials.Any(x => x.HasErrors);
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
        //проверить настройки подключения к СРЗ
        public void TestConnectionSRZ()
        {
            SrzConnectionIsValid = false;

            RemoveError(ErrorMessages.Connection, nameof(SrzAddress));
            SrzCredentials.ToList().ForEach(x => x.RemoveErrorsMessage(ErrorMessages.Connection));

            TestConnectionProxy();

            if (!ProxyConnectionIsValid)
                return;

            if (!TryConnectSite(SrzAddress, nameof(SrzAddress)))
                return;

            if (!TryAuthorizeSrzCredentials())
                return;

            SrzConnectionIsValid = true;
        }
        //устанавливает по-умолчанию настройки подключения к СРЗ
        public void SetDefaultSRZ()
        {
            SrzAddress = @"http://10.0.0.201/";
            SrzThreadsLimit = 10;
            srzRequestsLimit = 1000;
            FormatPatientsFile = true;
            SrzCredentials = new ObservableCollection<Credential>()
             {
                    new Credential{Login="МойЛогин1", Password="МойПароль1"},
                    new Credential{Login="МойЛогин2", Password="МойПароль2"},
                    new Credential{Login="МойЛогин3", Password="МойПароль3"}
             };
        }
        //устанавливает по-умолчанию настройки файла прикрепленных пациентов
        public void SetDefaultAttachedPatientsFile()
        {
            DownloadNewPatientsFile = true;
            PatientsFilePath = "Прикрепленные пациенты выгрузка.xlsx";

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
        #endregion

        #region Загрузка осмотров
        private string examinationsAddress;
        private byte examinationsThreadsLimit;
        private string examinationsFileNames;
        private string patientsFileNames;
        private ObservableCollection<Credential> examinationsCredentials;
        private string examinationsFileDirectory;
        private bool examinationsConnectionIsValid;

        public string ExaminationsAddress { get => examinationsAddress; set => SetProperty(ref examinationsAddress, FixUrl(value)); }
        public byte ExaminationsThreadsLimit { get => examinationsThreadsLimit; set => SetProperty(ref examinationsThreadsLimit, value); }
        public string ExaminationFileNames { get => examinationsFileNames; set => SetProperty(ref examinationsFileNames, value); }
        public string PatientFileNames { get => patientsFileNames; set => SetProperty(ref patientsFileNames, value); }
        public ObservableCollection<Credential> ExaminationsCredentials { get => examinationsCredentials; set => SetProperty(ref examinationsCredentials, value); }
        public string ExaminationsFileDirectory { get => examinationsFileDirectory; set => SetProperty(ref examinationsFileDirectory, value); }
        [XmlIgnore] public string FomsCodeMO { get; private set; }
        [XmlIgnore] public bool ExaminationsConnectionIsValid { get => examinationsConnectionIsValid; set => SetProperty(ref examinationsConnectionIsValid, value); }

        //проверяет учетные данные портала диспансеризации, в случае успеха - true
        private bool TryAuthorizeExaminationsCredentials()
        {
            var codesMO = new ConcurrentBag<string>();

            Parallel.ForEach(ExaminationsCredentials, new ParallelOptions { MaxDegreeOfParallelism = ExaminationsThreadsLimit }, credential =>
            {
                using (var service = new ExaminationService(ExaminationsAddress, UseProxy, ProxyAddress, ProxyPort))
                {
                    if (service.Authorize(credential))
                    {
                        codesMO.Add(service.FomsCodeMO);

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

            var uniqCodes = codesMO.ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (uniqCodes.Count > 1)
                throw new InvalidOperationException("Ошибка: учетные записи не должны принадлежать разным ЛПУ");

            FomsCodeMO = uniqCodes.FirstOrDefault();

            return !ExaminationsCredentials.Any(x => x.HasErrors);
        }
        //Устанавливает значения по умолчанию для портала диспансеризации
        public void SetDefaultExaminations()
        {
            ExaminationsAddress = @"http://10.0.0.203/";
            ExaminationsThreadsLimit = 5;
            PatientFileNames = @"LPM, LVM, LOM";
            ExaminationFileNames = @"DPM, DVM, DOM";

            ExaminationsCredentials = new ObservableCollection<Credential>()
             {
                    new Credential{Login="МойЛогин1", Password="МойПароль1"},
                    new Credential{Login="МойЛогин2", Password="МойПароль2"},
                    new Credential{Login="МойЛогин3", Password="МойПароль3"}
             };
        }
        //проверить настройеки подключения к порталу диспансризации
        public void TestConnectionExaminations()
        {
            ExaminationsConnectionIsValid = false;

            RemoveError(ErrorMessages.Connection, nameof(ExaminationsAddress));
            ExaminationsCredentials.ToList().ForEach(x => x.RemoveErrorsMessage(ErrorMessages.Connection));

            TestConnectionProxy();

            if (!ProxyConnectionIsValid)
                return;

            if (!TryConnectSite(ExaminationsAddress, nameof(ExaminationsAddress)))
                return;

            if (!TryAuthorizeExaminationsCredentials())
                return;

            ExaminationsConnectionIsValid = true;
        }
        #endregion
    }
}
