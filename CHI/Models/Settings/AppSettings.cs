using CHI.Infrastructure;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CHI.Models.Settings
{
    public class AppSettings : DomainObject
    {
        static readonly int timeoutConnection = 3000;
        static readonly string settingsFileName = "Settings.xml";
        Common common;
        SRZ srz;
        MedicalExaminations medicalExaminations;
        AttachedPatients attachedPatients;
        ServiceAccounting serviceAccounting;

        public Common Common { get => common; set => SetProperty(ref common, value); }
        public SRZ Srz { get => srz; set => SetProperty(ref srz, value); }
        public MedicalExaminations MedicalExaminations { get => medicalExaminations; set => SetProperty(ref medicalExaminations, value); }
        public AttachedPatients AttachedPatients { get => attachedPatients; set => SetProperty(ref attachedPatients, value); }
        public ServiceAccounting ServiceAccounting { get => serviceAccounting; set => SetProperty(ref serviceAccounting, value); }
        [XmlIgnore] public bool FailedToDecrypt { get; set; }
        [XmlIgnore] public string BackupSettingsFile { get; set; }


        public AppSettings()
        {
            Common = new();
            Srz = new();
            MedicalExaminations = new();
            AttachedPatients = new();
            ServiceAccounting = new();
        }


        //сохраняет настройки в xml
        public void Save()
        {
            Common.ProxyConnectionIsValid = false;
            Srz.ConnectionIsValid = false;
            MedicalExaminations.ConnectionIsValid = false;

            // Т.к. при создании экзмпляра класса если свойства не инициализируются - не срабатывает валидация. Поэтому принудительно проверяем все. 
            // Свойства не инициализируются сразу т.к. иначе сразу после создания на них будут отображаться ошибки, и во View тоже, это плохо.
            Srz.Credential.Validate();
            Srz.Credential.Encrypt(Common.CredentialsScope);

            MedicalExaminations.Credential.Validate();
            MedicalExaminations.Credential.Encrypt(Common.CredentialsScope);

            foreach (var columnProperty in AttachedPatients.ColumnProperties)
                columnProperty.Validate();

            using (var stream = new FileStream(settingsFileName, FileMode.Create))
            {
                var formatter = new XmlSerializer(GetType());
                formatter.Serialize(stream, this);
            }
        }

        //загружает настройки из xml
        public static AppSettings Load()
        {
            AppSettings settings;

            if (File.Exists(settingsFileName))
            {
                using (var stream = new FileStream(settingsFileName, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(Settings));
                    settings = formatter.Deserialize(stream) as Settings;
                }

                try
                {
                    settings.Srz.Credential.Decrypt(settings.Common.CredentialsScope);

                    settings.MedicalExaminations.Credential.Decrypt(settings.Common.CredentialsScope);
                }
                catch (CryptographicException)
                {
                    settings.Srz.Credential = new();
                    settings.MedicalExaminations.Credential = new();
                    settings.BackupSettingsFile = $@"Settings backup {DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF")}.xml";
                    settings.FailedToDecrypt = true;

                    File.Copy(settingsFileName, settings.BackupSettingsFile);

                    return settings;
                }
            }

            else
            {
                settings = new Settings();
                settings.SetDefault();
            }

            return settings;
        }

        public void SetDefault()
        {
            Common.SetDefault();
            Srz.SetDefault();
            MedicalExaminations.SetDefault();
            AttachedPatients.SetDefault();
            ServiceAccounting.SetDefault();
        }

        //исправляет url
        public static string FixUrl(string url)
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

        //проверяет настройки прокси-сервера, true-соединение установлено или прокси-сервер не используется, false-иначе
        public void TestConnectionProxy()
        {
            var connected = false;

            if (Common.UseProxy)
                using (var client = new TcpClient())
                {
                    try
                    {
                        client.ConnectAsync(Common.ProxyAddress, Common.ProxyPort).Wait(timeoutConnection);
                        connected = client.Connected;
                    }
                    catch (Exception)
                    { }
                }

            if (Common.UseProxy && !connected)
            {
                Common.AddError(ErrorMessages.Connection, nameof(Common.ProxyAddress));
                Common.AddError(ErrorMessages.Connection, nameof(Common.ProxyPort));
                Srz.ConnectionIsValid = false;
                MedicalExaminations.ConnectionIsValid = false;

                Common.ProxyConnectionIsValid = false;
            }
            else
            {
                Common.RemoveError(ErrorMessages.Connection, nameof(Common.ProxyAddress));
                Common.RemoveError(ErrorMessages.Connection, nameof(Common.ProxyPort));

                Common.ProxyConnectionIsValid = true;
            }
        }

        //проверяет доступность сайта, в случае успеха - true
        bool TryConnectSite(string url, string nameOfAddress)
        {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Timeout = timeoutConnection;

                if (Common.UseProxy)
                    webRequest.Proxy = new WebProxy(Common.ProxyAddress + ":" + Common.ProxyPort);

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

        //проверить настройеки подключения к порталу диспансризации
        public async Task TestConnectionExaminationsAsync()
        {
            ExaminationsConnectionIsValid = false;

            RemoveError(ErrorMessages.Connection, nameof(ExaminationsAddress));
            ExaminationsCredentials.ToList().ForEach(x => x.RemoveErrorsMessage(ErrorMessages.Authorization));

            TestConnectionProxy();

            if (!ProxyConnectionIsValid)
                return;

            if (!TryConnectSite(ExaminationsAddress, nameof(ExaminationsAddress)))
                return;

            if (!await TryAuthorizeExaminationsCredentialsAsync())
                return;

            ExaminationsConnectionIsValid = true;
        }

        //проверить настройки подключения к СРЗ
        public async Task TestConnectionSRZAsync()
        {
            SrzConnectionIsValid = false;

            RemoveError(ErrorMessages.Connection, nameof(SrzAddress));
            SrzCredentials.ToList().ForEach(x => x.RemoveErrorsMessage(ErrorMessages.Authorization));

            TestConnectionProxy();

            if (!ProxyConnectionIsValid)
                return;

            if (!TryConnectSite(SrzAddress, nameof(SrzAddress)))
                return;

            if (!await TryAuthorizeSrzCredentialsAsync())
                return;

            SrzConnectionIsValid = true;
        }

        //проверяет учетные данные СРЗ, в случае успеха - true
        async Task<bool> TryAuthorizeSrzCredentialsAsync()
        {
            var credential = SrzCredentials.First();

            using var service = new SRZService(SrzAddress, UseProxy, ProxyAddress, ProxyPort);

            var isAuthorized = false;
            try
            {
                isAuthorized = await service.AuthorizeAsync(credential);
            }
            catch (Exception)
            {
            }

            if (service.IsAuthorized)
            {
                await service.LogoutAsync();

                credential.RemoveErrors(nameof(credential.Login));
                credential.RemoveErrors(nameof(credential.Password));
            }
            else
            {
                credential.AddError(ErrorMessages.Authorization, nameof(credential.Login));
                credential.AddError(ErrorMessages.Authorization, nameof(credential.Password));
            }

            return isAuthorized;
        }

        //проверяет учетные данные портала диспансеризации, в случае успеха - true
        async Task<bool> TryAuthorizeExaminationsCredentialsAsync()
        {
            var credential = ExaminationsCredentials.First();

            using var service = new ExaminationService(ExaminationsAddress, UseProxy, ProxyAddress, ProxyPort);

            var isAuthorized = false;

            try
            {
                isAuthorized = await service.AuthorizeAsync(credential);
            }
            catch (Exception)
            {
            }

            if (service.IsAuthorized)
            {
                FomsCodeMO = service.FomsCodeMO;

                await service.LogoutAsync();

                credential.RemoveErrors(nameof(credential.Login));
                credential.RemoveErrors(nameof(credential.Password));

            }
            else
            {
                credential.AddError(ErrorMessages.Authorization, nameof(credential.Login));
                credential.AddError(ErrorMessages.Authorization, nameof(credential.Password));
            }

            return isAuthorized;
        }
    }
}
