namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет информацию о списке дальнейших возможных шагов
    /// </summary>
    public class AvailableStage
    {
        /// <summary>
        /// Текущий шаг осмотра
        /// </summary>
        public StepKind StageId { get; set; }
        /// <summary>
        /// Следующий шаг осмотра
        /// </summary>
        public int NextStageId { get; set; }
        /// <summary>
        /// Предыдущий шаг осмотра
        /// </summary>
        public object PreviousStageId { get; set; }
    }
}
