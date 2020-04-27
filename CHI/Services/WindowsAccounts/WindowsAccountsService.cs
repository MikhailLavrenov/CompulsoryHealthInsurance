using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace CHI.Services.WindowsAccounts
{
    /// <summary>
    /// Сервис для получения списков локальных и доменных (Active Directory) пользователей
    /// </summary>
    public class WindowsAccountsService
    {
        /// <summary>
        /// Домен  доступен по сети
        /// </summary>
        public bool IsDomainAvailable { get; }
        /// <summary>
        /// Список локальных пользователей
        /// </summary>
        public List<WindowsAccount> Local { get; }
        /// <summary>
        /// Список доменных пользователей
        /// </summary>
        public List<WindowsAccount> Domain { get; }


        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <param name="DomainName">Имя домена</param>
        /// <param name="domainUsersRootOU">Корневой OU с которого начнется поиск учетных записей</param>
        public WindowsAccountsService(string domainName, string domainUsersRootOU = null)
        {
            if (!string.IsNullOrEmpty(domainName))
            {
                IsDomainAvailable = true;

                var domainPrefix = domainName.Substring(0, domainName.IndexOf('.')).ToUpper();

                var filter = (string)null;

                if (!string.IsNullOrEmpty(domainUsersRootOU))
                {
                    var filterBuilder = new StringBuilder($"OU={domainUsersRootOU}");

                    foreach (var item in domainName.Split('.'))
                        filterBuilder.Append($",DC={item}");

                    filter = filterBuilder.ToString();
                }

                using var dContext = new PrincipalContext(ContextType.Domain, domainName, filter);

                Domain = GetUsers(dContext, domainPrefix);
            }
            else
                Domain = new List<WindowsAccount>();

            using var context = new PrincipalContext(ContextType.Machine);

            Local = GetUsers(context, Environment.MachineName);
        }


        /// <summary>
        /// Создает список локальных или доменных пользователей 
        /// </summary>
        /// <param name="contextType">Значение перечисления ContextType - задает расположение учетных записей: Локальная или Доменная</param>
        /// <param name="OUName">Название Organizational Unit для доменных учетных записей</param>
        public List<WindowsAccount> GetUsers(PrincipalContext context, string prefix)
        {
            using var searcher = new PrincipalSearcher(new UserPrincipal(context));

            try
            {
                return searcher
                    .FindAll()
                    .Select(x => new WindowsAccount(x.Name, $"{prefix}\\{x.SamAccountName}", x.Sid.ToString()))
                    .OrderBy(x => x.Name)
                    .ToList();
            }
            catch
            { }

            return new List<WindowsAccount>();
        }
    }
}
