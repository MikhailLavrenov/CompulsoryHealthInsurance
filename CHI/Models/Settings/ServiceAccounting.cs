using CHI.Infrastructure;
using System;

namespace CHI.Models.Settings
{
    public class ServiceAccounting : DomainObject
    {
        
        string domainName;
        string domainUsersRootOU;
        string approvedBy;
        string reportPath;

        
        public string DomainName { get => domainName; set => SetProperty(ref domainName, value); }
        public string DomainUsersRootOU { get => domainUsersRootOU; set => SetProperty(ref domainUsersRootOU, value); }
        public string ApprovedBy { get => approvedBy; set => SetProperty(ref approvedBy, value); }
        public string ReportPath { get => reportPath; set => SetProperty(ref reportPath, value); }


        public void SetDefault()
        {
            DomainName = "poliklinika.local";
            DomainUsersRootOU = "Users";
            ApprovedBy = $"Главный врач{Environment.NewLine}Поликлиники{Environment.NewLine}Иванов А.П.";
            CredentialsScope = CredentialScope.ТекущийПользователь;
        }
    }
}
