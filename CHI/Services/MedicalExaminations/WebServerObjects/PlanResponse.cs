using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет ответ веб-сервера при поиске пациента в плане осмотров
    /// </summary>
    public class PlanResponse
    {
        /// <summary>
        /// Список WebPatientData
        /// </summary>
        public List<WebPatientData> Data { get; set; }
    }
}
