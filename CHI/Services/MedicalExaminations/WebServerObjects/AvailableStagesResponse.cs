using System.Collections.Generic;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет ответ веб-сервера о списке дальнейших возможных шагов
    /// </summary>
    public class AvailableStagesResponse
    {
        /// <summary>
        /// Список дальнейших возможных шагов
        /// </summary>
        public List<AvailableStage> AvailableStages { get; set; }
    }
}
