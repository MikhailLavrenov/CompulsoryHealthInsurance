using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services
{
    /// <summary>
    /// Интерфейс учетных данных
    /// </summary>
    public interface ICredential
    {
        /// <summary>
        /// Логин
        /// </summary>
         string Login { get; }
        /// <summary>
        /// Пароль
        /// </summary>
         string Password { get; }
    }
}
