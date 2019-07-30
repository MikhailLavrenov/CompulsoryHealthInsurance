using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
{
    public class Credential : BindableBase
    {
        #region Fields
        private readonly object locker = new object();
        private string login;
        private string password;
        private int requestsLimit;
        private int requestsLeft;

        private bool isNotValid;
        #endregion

        #region Properties
        [XmlIgnore] public string Login { get => login; set => SetProperty(ref login, value); }
        [XmlIgnore] public string Password { get => password; set => SetProperty(ref password, value); }
        public int RequestsLimit
        {
            get => requestsLimit;
            set
            {
                SetProperty(ref requestsLimit, value);
                requestsLeft = value;
            }
        }

        [XmlIgnore] public bool IsNotValid { get => isNotValid; set => SetProperty(ref isNotValid, value); }
        #endregion

        #region Creators
        //создает копию экземпляра класса
        public Credential Copy()
        {
            return MemberwiseClone() as Credential;
        }
        #endregion

        #region Methods
        //попытка зарезервировать разрешение на запрос к серверу
        public bool TryReserveRequest()
        {
            lock (locker)
            {
                if (requestsLeft != 0)
                {
                    requestsLeft--;
                    return true;
                }
                else
                    return false;
            }
        }
        //шифрует текст в соответствии с видимостью
        private string Encrypt(string text, СredentialScope сredentialScope)   
        {
            if (сredentialScope== СredentialScope.Все)
                return text;           

            DataProtectionScope scope;
            if (сredentialScope == СredentialScope.ТекущийПользователь)
                scope = DataProtectionScope.CurrentUser;
            else
                scope = DataProtectionScope.LocalMachine;

            byte[] byteText = Encoding.Default.GetBytes(text);
            var protectedText = ProtectedData.Protect(byteText, null, scope);

            return Convert.ToBase64String(protectedText);
        }
        //расшифровывает текст в соответствии с видимостью
        private string Decrypt(string text, СredentialScope сredentialScope)
        {
            if (сredentialScope == СredentialScope.Все)
                return text;          

            DataProtectionScope scope;
            if (сredentialScope == СredentialScope.ТекущийПользователь)
                scope = DataProtectionScope.CurrentUser;
            else
                scope = DataProtectionScope.LocalMachine;
           
            try
            {
                byte[] byteText = Convert.FromBase64String(text);
                var unprotectedText = ProtectedData.Unprotect(byteText, null, scope);
                return Encoding.Default.GetString(unprotectedText);
            }
            catch (Exception)
            {
                return "";
            }
        }
        //пытается расшифровывает текст в соответствии с видимостью
        private bool TryDecrypt(string text, СredentialScope сredentialScope, out string decryptedText)
        {
            if (сredentialScope == СredentialScope.Все)
            {
                decryptedText = text;
                return true;
            }

            DataProtectionScope scope;
            if (сredentialScope == СredentialScope.ТекущийПользователь)
                scope = DataProtectionScope.CurrentUser;
            else
                scope = DataProtectionScope.LocalMachine;

            try
            {
                byte[] byteText = Convert.FromBase64String(text);
                var unprotectedText = ProtectedData.Unprotect(byteText, null, scope);
                decryptedText= Encoding.Default.GetString(unprotectedText);
                return true;
            }
            catch (Exception)
            {
                decryptedText = null;
                return false;
            }
        }
        #endregion

    }
}
