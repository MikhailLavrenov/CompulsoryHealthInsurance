using CHI.Services;
using PatientsFomsRepository.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace PatientsFomsRepository.Models
{
    public class Credential : DomainObject,ICredential
    {
        #region Поля
        private readonly object locker = new object();
        private string login;
        private string password;
        private uint requestsLimit;
        private uint requestsLeft;
        #endregion

        #region Свойства
        public static CredentialScope Scope { get; set; }
        [XmlIgnore] public string Login { get => login; set => SetProperty(ref login, value); }
        public string ProtectedLogin { get => Encrypt(Login); set => Login = Decrypt(value); }
        [XmlIgnore] public string Password { get => password; set => SetProperty(ref password, value); }
        public string ProtectedPassword { get => Encrypt(Password); set => Password = Decrypt(value); }
        public uint RequestsLimit
        {
            get => requestsLimit;
            set
            {
                SetProperty(ref requestsLimit, value);
                requestsLeft = value;
            }
        }
        #endregion

        #region Конструкторы
        #endregion

        #region Методы
        //создает копию экземпляра класса
        public Credential Copy()
        {
            return MemberwiseClone() as Credential;
        }
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
        //валидация свойств
        public override void Validate(string propertyName=null)
        {
            if (propertyName == nameof(Login) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Login), Login);

            if (propertyName == nameof(Password) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Password), Password);
        }
        //шифрует текст в соответствии с видимостью
        private static string Encrypt(string text)
        {
            if (text == null)
                text = "";

            if (Scope == CredentialScope.Все)
                return text;

            DataProtectionScope scope;
            if (Scope == CredentialScope.ТекущийПользователь)
                scope = DataProtectionScope.CurrentUser;
            else
                scope = DataProtectionScope.LocalMachine;

            byte[] byteText = Encoding.Default.GetBytes(text);
            var protectedText = ProtectedData.Protect(byteText, null, scope);

            return Convert.ToBase64String(protectedText);
        }
        //расшифровывает текст в соответствии с видимостью
        private static string Decrypt(string text)
        {
            if (text == null)
                text = string.Empty;

            if (Scope == CredentialScope.Все)
                return text;

            DataProtectionScope scope;
            if (Scope == CredentialScope.ТекущийПользователь)
                scope = DataProtectionScope.CurrentUser;
            else
                scope = DataProtectionScope.LocalMachine;

            try
            {
                var byteText = Convert.FromBase64String(text);
                var unprotectedText = ProtectedData.Unprotect(byteText, null, scope);
                return Encoding.Default.GetString(unprotectedText);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
