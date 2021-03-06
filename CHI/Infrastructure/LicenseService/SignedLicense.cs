﻿using System;

namespace CHI.Infrastructure
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
