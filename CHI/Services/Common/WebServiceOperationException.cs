using System;

namespace CHI.Services.Common
{
    /// <summary>
    /// Исключение, которое выдается при возврате web-сервером ответа с сообщением об ошибке.
    /// </summary>
    public class WebServiceOperationException : ApplicationException
    {
        private static readonly string defaultErrorMessage = "Произошла ошибка выполнения операции на стороне web-сервер";


        public WebServiceOperationException()
            : this(defaultErrorMessage)
        { }

        public WebServiceOperationException(string message) : base(message)
        { }
    }
}
