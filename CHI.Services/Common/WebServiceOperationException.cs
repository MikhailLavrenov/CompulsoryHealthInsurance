using System;

namespace CHI.Services.Common
{
    /// <summary>
    /// Исключение, которое выдается при возврате web-сервером ответа с сообщением об ошибке.
    /// </summary>
    public class WebServiceOperationException : ApplicationException
    {
        private static readonly string defaultErrorMessage = "Произошла ошибка выполнения операции на стороне web-сервер";

        /// <summary>
        /// Конструктор по-умолчанию. Использует стандартное сообщение об ошибке.
        /// </summary>
        public WebServiceOperationException()
            : this(defaultErrorMessage)
        { }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        public WebServiceOperationException(string message) : base(message)
        { }
    }
}
