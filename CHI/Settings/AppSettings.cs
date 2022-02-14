using CHI.Infrastructure;
using CHI.Services.MedicalExaminations;
using CHI.Services.SRZ;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CHI.Settings
{
    public class AppSettings : DomainObject
    {
        static readonly string fileName = "Settings.xml";
        CommonSettings common;
        SrzSettings srz;
        MedicalExaminationsSettings medicalExaminations;
        AttachedPatientsFileSettings attachedPatientsFile;
        ServiceAccountingSettings serviceAccounting;


        public CommonSettings Common { get => common; set => SetProperty(ref common, value); }
        public SrzSettings Srz { get => srz; set => SetProperty(ref srz, value); }
        public MedicalExaminationsSettings MedicalExaminations { get => medicalExaminations; set => SetProperty(ref medicalExaminations, value); }
        public AttachedPatientsFileSettings AttachedPatientsFile { get => attachedPatientsFile; set => SetProperty(ref attachedPatientsFile, value); }
        public ServiceAccountingSettings ServiceAccounting { get => serviceAccounting; set => SetProperty(ref serviceAccounting, value); }
        [XmlIgnore] public bool FailedToDecrypt { get; set; }
        [XmlIgnore] public string BackupSettingsFile { get; set; }


        public AppSettings()
        {
            Common = new();
            Srz = new();
            MedicalExaminations = new();
            AttachedPatientsFile = new();
            ServiceAccounting = new();
        }


        public void Save()
        {
            Common.ProxyConnectionIsValid = false;
            Srz.ConnectionIsValid = false;
            MedicalExaminations.ConnectionIsValid = false;

            // Т.к. при создании экзмпляра класса если свойства не инициализируются - не срабатывает валидация. Поэтому принудительно проверяем все. 
            // Свойства не инициализируются сразу т.к. иначе сразу после создания на них будут отображаться ошибки, и во View тоже, это плохо.
            Srz.Credential?.Validate();
            Srz.Credential?.Encrypt(Common.CredentialsScope);

            MedicalExaminations.Credential?.Validate();
            MedicalExaminations.Credential?.Encrypt(Common.CredentialsScope);

            foreach (var columnProperty in AttachedPatientsFile.ColumnProperties)
                columnProperty.Validate();

            using var stream = new FileStream(fileName, FileMode.Create);
            var formatter = new XmlSerializer(GetType());
            formatter.Serialize(stream, this);
        }

        public static AppSettings Load()
        {
            AppSettings settings;

            if (File.Exists(fileName))
            {
                using (var stream = new FileStream(fileName, FileMode.Open))
                {
                    var formatter = new XmlSerializer(typeof(AppSettings));
                    settings = formatter.Deserialize(stream) as AppSettings;
                }

                try
                {
                    settings.Srz.Credential?.Decrypt(settings.Common.CredentialsScope);

                    settings.MedicalExaminations.Credential?.Decrypt(settings.Common.CredentialsScope);
                }
                catch (CryptographicException)
                {
                    settings.Srz.Credential = new();
                    settings.MedicalExaminations.Credential = new();
                    settings.BackupSettingsFile = $"Settings backup {DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_FFF")}.xml";
                    settings.FailedToDecrypt = true;

                    File.Copy(fileName, settings.BackupSettingsFile);

                    return settings;
                }
            }

            else
            {
                settings = new AppSettings();
                //settings.SetDefault();
            }

            return settings;
        }

        public void SetDefault()
        {
            Common.SetDefault();
            Srz.SetDefault();
            MedicalExaminations.SetDefault();
            AttachedPatientsFile.SetDefault();
            ServiceAccounting.SetDefault();
        }

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

        public async Task TestConnectionProxyAsync()
        {
            var connected = false;

            if (Common.UseProxy)
            {
                using var client = new TcpClient();

                client.ReceiveTimeout = CommonSettings.TimeoutConnection;
                client.SendTimeout = CommonSettings.TimeoutConnection;

                CancellationTokenSource cts = new();
                cts.CancelAfter(CommonSettings.TimeoutConnection);
                var token = cts.Token;

                try
                {
                    await client.ConnectAsync(Common.ProxyAddress, Common.ProxyPort, token);
                    connected = client.Connected;
                }
                catch (Exception)
                { }


                if (!connected)
                {
                    Common.AddError(ErrorMessages.Connection, nameof(Common.ProxyAddress));
                    Common.AddError(ErrorMessages.Connection, nameof(Common.ProxyPort));
                    Srz.ConnectionIsValid = false;
                    MedicalExaminations.ConnectionIsValid = false;

                    Common.ProxyConnectionIsValid = false;
                }
            }
            else
            {
                Common.RemoveError(ErrorMessages.Connection, nameof(Common.ProxyAddress));
                Common.RemoveError(ErrorMessages.Connection, nameof(Common.ProxyPort));

                Common.ProxyConnectionIsValid = true;
            }
        }

        async Task<bool> TryConnectSiteAsync(string url, string nameOfAddress, DomainObject obj)
        {
            try
            {
                IWebProxy proxy = Common.UseProxy ? null : new WebProxy(Common.Proxy);
                using var handler = new HttpClientHandler() { Proxy = proxy, UseProxy = Common.UseProxy };
                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMilliseconds(CommonSettings.TimeoutConnection);

                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                obj.RemoveError(ErrorMessages.Connection, nameOfAddress);
                return true;
            }
            catch (Exception)
            {
                obj.AddError(ErrorMessages.Connection, nameOfAddress);
                return false;
            }
        }

        public async Task TestConnectionExaminationsAsync()
        {
            MedicalExaminations.ConnectionIsValid = false;

            MedicalExaminations.RemoveError(ErrorMessages.Connection, nameof(MedicalExaminations.Address));
            MedicalExaminations.Credential.RemoveErrorsMessage(ErrorMessages.Authorization);

            await TestConnectionProxyAsync();

            if (!Common.ProxyConnectionIsValid)
                return;

            if (! await TryConnectSiteAsync(MedicalExaminations.Address, nameof(MedicalExaminations.Address), MedicalExaminations))
                return;

            if (!await TryAuthorizeExaminationsCredentialsAsync())
                return;

            MedicalExaminations.ConnectionIsValid = true;
        }

        public async Task TestConnectionSRZAsync()
        {
            Srz.ConnectionIsValid = false;

            Srz.RemoveError(ErrorMessages.Connection, nameof(Srz.Address));
            Srz.Credential.RemoveErrorsMessage(ErrorMessages.Authorization);

            await TestConnectionProxyAsync();

            if (!Common.ProxyConnectionIsValid)
                return;

            if (! await TryConnectSiteAsync(Srz.Address, nameof(Srz.Address), Srz))
                return;

            if (!await TryAuthorizeSrzCredentialsAsync())
                return;

            Srz.ConnectionIsValid = true;
        }

        async Task<bool> TryAuthorizeSrzCredentialsAsync()
        {
            var credential = Srz.Credential;

            using var service = new SRZService(Srz.Address, Common.UseProxy, Common.ProxyAddress, Common.ProxyPort);

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

        async Task<bool> TryAuthorizeExaminationsCredentialsAsync()
        {
            var credential = MedicalExaminations.Credential;

            using var service = new ExaminationService(MedicalExaminations.Address, Common.UseProxy, Common.ProxyAddress, Common.ProxyPort);

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
                MedicalExaminations.FomsCodeMO = service.FomsCodeMO;

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
