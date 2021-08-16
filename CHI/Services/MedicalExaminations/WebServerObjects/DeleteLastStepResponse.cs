using System.Collections.Generic;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет ответ веб-сервера при удалении последнго шага осмотра
    /// </summary>
    public class DeleteLastStepResponse
    {
        /// <summary>
        /// Возникла ошибка, true-ошибка, false-без ошибок.
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Список выполненных шагов
        /// </summary>
        public List<StepData> Data { get; set; }
    }
}
