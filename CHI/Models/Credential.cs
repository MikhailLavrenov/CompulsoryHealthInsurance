using CHI.Infrastructure;
using CHI.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CHI.Application.Models
{
    public class Credential : DomainObject, ICredential
    {
        #region Поля
        private string login;
        private string password;
        #endregion

        #region Свойства
        [XmlIgnore] public string Login { get => login; set => SetProperty(ref login, value); }        
        [XmlIgnore] public string Password { get => password; set => SetProperty(ref password, value); }
        public string ProtectedLogin { get; set; }
        public string ProtectedPassword { get; set; }
        #endregion

        #region Конструкторы
        #endregion

        #region Методы
        //валидация свойств
        public override void Validate(string propertyName = null)
        {
            if (propertyName == nameof(Login) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Login), Login);

            if (propertyName == nameof(Password) || propertyName == null)
                ValidateIsNullOrEmptyString(nameof(Password), Password);
        }
        //шифрует учетные данные в соответствии с видимостью
        public void Encrypt(CredentialScope scope)
        {
            ProtectedLogin = InternalEncrypt(Login, scope);
            ProtectedPassword = InternalEncrypt(Password, scope);
        }
        //расшифровывает текст в соответствии с видимостью
        public void Decrypt(CredentialScope scope)
        {
            Login = InternalDecrypt(ProtectedLogin, scope);
            Password = InternalDecrypt(ProtectedPassword, scope);
        }
        //шифрует учетные данные в соответствии с видимостью
        private static string InternalEncrypt(string text, CredentialScope scope)
        {
            if (text == null)
                text = string.Empty;

            if (scope == CredentialScope.Все)
                return text;

            var protectionScope = scope == CredentialScope.ТекущийПользователь ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine;

            byte[] byteText = Encoding.Default.GetBytes(text);
            var protectedText = ProtectedData.Protect(byteText, null, protectionScope);

            return Convert.ToBase64String(protectedText);
        }
        //расшифровывает текст в соответствии с видимостью
        private static string InternalDecrypt(string text, CredentialScope scope)
        {
            if (text == null)
                text = string.Empty;

            if (scope == CredentialScope.Все)
                return text;

            var protectionScope = scope == CredentialScope.ТекущийПользователь ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine;

            var byteText = Convert.FromBase64String(text);
            var unprotectedText = ProtectedData.Unprotect(byteText, null, protectionScope);
            return Encoding.Default.GetString(unprotectedText);
        }
        #endregion
    }
}
