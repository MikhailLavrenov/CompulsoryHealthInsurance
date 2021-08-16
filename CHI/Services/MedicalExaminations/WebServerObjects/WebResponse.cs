using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет простой ответ веб-сервер о результате выполнения запроса
    /// </summary>
    public class WebResponse
    {
        /// <summary>
        /// Возникла ошибка, true-ошибка, false-без ошибок.
        /// </summary>
        public bool IsError { get; set; }
    }
}
