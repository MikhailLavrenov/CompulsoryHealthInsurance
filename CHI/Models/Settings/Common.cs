using CHI.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace CHI.Models
{
    public class Common : DomainObject
    {
        static readonly int timeoutConnection = 3000;
        bool useProxy;
        string proxyAddress;
        ushort proxyPort;
        bool proxyConnectionIsValid;
        bool useSQLServer;
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
        public bool UseSQLServer { get => useSQLServer; set => SetProperty(ref useSQLServer, value); }
        public string SQLServer { get => sqlServer; set => SetProperty(ref sqlServer, value); }
        public string SQLServerDB { get => sqlServerDB; set => SetProperty(ref sqlServerDB, value); }
        public CredentialScope CredentialsScope { get => credentialsScope; set => SetProperty(ref credentialsScope, value); }


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

        //устанавливает по-умолчанию настройки для прочих настроек
        public void SetDefault()
        {
            UseProxy = false;
            ProxyAddress = "";
            ProxyPort = 0;

            UseSQLServer = true;
            SQLServer = "yourServer\\istance";
            SQLServerDB = "CHI";
        }
    }
}
