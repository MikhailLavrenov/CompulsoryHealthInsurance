using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.Common
{
    public class WebServiceOperationException:ApplicationException
    {
        private static readonly string defaultErrorMessage="Произошла ошибка выполнения операции на стороне web-сервер";

        public WebServiceOperationException():this(defaultErrorMessage)
        { 
        }
        public WebServiceOperationException(string message):base (message)
        {
        }
    }
}
