using CHI.Infrastructure;
using CHI.Models;
using System.Xml.Serialization;

namespace CHI.Settings
{
    public class CommonSettings : DomainObject
    {
        static internal int TimeoutConnection { get; } = 3000;
        bool useProxy;
        string proxyAddress;
        ushort proxyPort;
        bool proxyConnectionIsValid;
        string sqlServer;
        string sqlServerDB;
        CredentialScope credentialsScope;


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
        public string ProxyAddress { get => proxyAddress; set => SetProperty(ref proxyAddress, value.Trim()); }
        public ushort ProxyPort { get => proxyPort; set => SetProperty(ref proxyPort, value); }
        [XmlIgnore] public bool ProxyConnectionIsValid { get => proxyConnectionIsValid; set => SetProperty(ref proxyConnectionIsValid, value); }
        [XmlIgnore] public string Proxy { get => $"{ProxyAddress}:{ProxyPort}"; }
        public string SQLServer { get => sqlServer; set => SetProperty(ref sqlServer, value); }
        public string SQLServerDB { get => sqlServerDB; set => SetProperty(ref sqlServerDB, value); }
        public CredentialScope CredentialsScope { get => credentialsScope; set => SetProperty(ref credentialsScope, value); }


        public CommonSettings()
        {
            CredentialsScope = CredentialScope.ТекущийПользователь;
        }


        public override void Validate(string propertyName)
        {
            switch (propertyName)
            {
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
            }
        }

        public void SetDefault()
        {
            UseProxy = false;
            ProxyAddress = "";
            ProxyPort = 0;

            SQLServer = "yourServer\\istance";
            SQLServerDB = "CHI";
        }
    }
}
