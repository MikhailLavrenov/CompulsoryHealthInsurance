using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Application.Models
{
    /// <summary>
    /// Представляет информацию подписанной о лицензии
    /// </summary>
    [Serializable]
    public class SignedLicense
    {
        /// <summary>
        /// Лицензия
        /// </summary>
        public License License { get; set; }
        /// <summary>
        /// Подпись лицензии
        /// </summary>
        public byte[] Sign { get; set; }
    }
}
