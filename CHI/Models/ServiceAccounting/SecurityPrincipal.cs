using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    /// <summary>
    /// Вспомогательный класс для получения списков локальных и доменных (Active Directory) пользователей и групп
    /// </summary>
    public class SecurityPrincipals
    {
        private string domainName;
        private string domainPrefix;

        /// <summary>
        /// Домен  доступен по сети
        /// </summary>
        public bool DomainAvailable { get; private set; }
        /// <summary>
        /// Список локальных пользователей
        /// </summary>
        public List<Principal> LocalUsers { get; private set; }
        /// <summary>
        /// Список локальных групп безопасности
        /// </summary>
        public List<Principal> LocalGroups { get; private set; }
        /// <summary>
        /// Список доменных пользователей
        /// </summary>
        public List<Principal> DomainUsers { get; private set; }
        /// <summary>
        /// Список доменных групп безопасности
        /// </summary>
        public List<Principal> DomainGroups { get; private set; }

        /// <summary>
        /// Конструктор класса, принимает Имя домена
        /// </summary>
        /// <param name="DomainName">Имя домена</param>
        public SecurityPrincipals(string domainName)
        {
            this.domainName = domainName;

            if (domainName.Length > 0)
            {
                DomainAvailable = true;
                domainPrefix = domainName.Substring(0, domainName.IndexOf('.')).ToUpper();
            }

            LocalUsers = new List<Principal>();
            LocalGroups = new List<Principal>();
            DomainUsers = new List<Principal>();
            DomainGroups = new List<Principal>();
        }
        /// <summary>
        /// Создает список локальных или доменных групп безопасности 
        /// </summary>
        /// <param name="contextType">Значение перечисления ContextType - задает расположение учетных записей: Локальная или Доменная</param>
        /// <param name="OUName">Название Organizational Unit для доменных учетных записей</param>
        public void SetGroups(ContextType contextType, string OUName = null)
        {
            var list = new List<Principal>();
            PrincipalContext context;
            PrincipalSearcher searcher;
            string prefix;

            if (contextType == ContextType.Domain)
            {
                if (DomainAvailable == false)
                    return;
                prefix = domainPrefix;
                context = new PrincipalContext(contextType, domainName, GetFilter(OUName));
            }
            else
            {
                prefix = Environment.MachineName;
                context = new PrincipalContext(contextType);
            }

            searcher = new PrincipalSearcher(new GroupPrincipal(context));
            try
            {
                foreach (GroupPrincipal group in searcher.FindAll())
                {
                    var item = new Principal();

                    item.Name = string.Format(@"{0}\{1}", prefix, group.Name);
                    item.Sid = group.Sid.ToString();

                    list.Add(item);
                }

                if (contextType == ContextType.Domain)
                    DomainGroups = list.OrderBy(x => x.Name).ToList();
                else
                    LocalGroups = list.OrderBy(x => x.Name).ToList();
            }
            catch { }
            finally
            {
                context.Dispose();
                searcher.Dispose();
            }
        }
        /// <summary>
        /// Создает список локальных или доменных пользователей 
        /// </summary>
        /// <param name="contextType">Значение перечисления ContextType - задает расположение учетных записей: Локальная или Доменная</param>
        /// <param name="OUName">Название Organizational Unit для доменных учетных записей</param>
        public void SetUsers(ContextType contextType, string OUName = null)
        {
            var list = new List<Principal>();
            PrincipalContext context;
            PrincipalSearcher searcher;
            string prefix;

            if (contextType == ContextType.Domain)
            {
                if (DomainAvailable == false)
                    return;
                prefix = domainPrefix;

                context = new PrincipalContext(contextType, domainName, GetFilter(OUName));
            }
            else
            {
                prefix = Environment.MachineName;
                context = new PrincipalContext(contextType);
            }

            searcher = new PrincipalSearcher(new UserPrincipal(context));

            try
            {
                foreach (UserPrincipal user in searcher.FindAll())
                {
                    var item = new Principal();

                    item.Name = string.Format(@"{0}\{1}", prefix, user.SamAccountName);
                    item.Sid = user.Sid.ToString();
                    list.Add(item);
                }
                if (contextType == ContextType.Domain)
                    DomainUsers = list.OrderBy(x => x.Name).ToList();
                else
                    LocalUsers = list.OrderBy(x => x.Name).ToList();
            }
            catch { }
            finally
            {
                context.Dispose();
                searcher.Dispose();
            }
        }
        /// <summary>
        /// Формирует строку для фильтра в соответствии с доменом и заданным OU
        /// </summary>
        /// <param name="OU">Название Organizational Unit</param>
        /// <returns></returns>
        string GetFilter(string OU)
        {
            var filter = new StringBuilder();

            if (OU == "")
                return null;

            filter.Append(string.Format("OU={0},", OU));

            foreach (var item in domainName.Split('.'))
                filter.Append(string.Format("DC={0},", item));
            filter.Remove(filter.Length - 1, 1);

            return filter.ToString();
        }
        /// <summary>
        /// Класс учетной записи пользователя или группые
        /// </summary>
        public class Principal
        {
            public string Name { get; set; }
            public string Sid { get; set; }
        }
    }
}
