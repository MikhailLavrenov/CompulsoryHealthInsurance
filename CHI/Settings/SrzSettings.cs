using CHI.Infrastructure;
using CHI.Models;
using System;
using System.Xml.Serialization;

namespace CHI.Settings
{
    public class SrzSettings : DomainObject
    {
        string address;
        byte maxDegreeOfParallelism;
        uint srzRequestsLimit;
        Credential srzCredential;
        bool connectionIsValid;
        bool downloadNewPatientsFile;


        public string Address { get => address; set => SetProperty(ref address, AppSettings.FixUrl(value)); }
        public byte MaxDegreeOfParallelism { get => maxDegreeOfParallelism; set => SetProperty(ref maxDegreeOfParallelism, value); }
        public uint RequestsLimit { get => srzRequestsLimit; set => SetProperty(ref srzRequestsLimit, value); }
        public Credential Credential { get => srzCredential; set => SetProperty(ref srzCredential, value); }
        [XmlIgnore] public bool ConnectionIsValid { get => connectionIsValid; set => SetProperty(ref connectionIsValid, value); }
        public bool DownloadNewPatientsFile { get => downloadNewPatientsFile; set => SetProperty(ref downloadNewPatientsFile, value); }


        public override void Validate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Address):
                    if (string.IsNullOrEmpty(Address) || Uri.TryCreate(Address, UriKind.Absolute, out _) == false)
                        AddError(ErrorMessages.UriFormat, propertyName);
                    else
                        RemoveError(ErrorMessages.UriFormat, propertyName);
                    break;

                case nameof(MaxDegreeOfParallelism):
                    if (MaxDegreeOfParallelism < 1)
                        AddError(ErrorMessages.LessOne, propertyName);
                    else
                        RemoveError(ErrorMessages.LessOne, propertyName);
                    break;
            }
        }

        public void SetDefault()
        {
            Address = @"http://srz.foms.local/";
            MaxDegreeOfParallelism = 10;
            RequestsLimit = 1000;

            Credential = new Credential { Login = "МойЛогин", Password = "МойПароль" };
            DownloadNewPatientsFile = true;
        }
    }
}
