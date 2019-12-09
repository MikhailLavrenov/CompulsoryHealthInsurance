namespace CHI.Services.Common
{
    /// <summary>
    /// Аргументы события изменение счетчика
    /// </summary>
    public struct CounterEventArgs
    {
        /// <summary>
        /// Текущее значение счетчика
        /// </summary>
        public int Counter { get; set; }
        /// <summary>
        /// Итоговое значение счетчика
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Контсруктор
        /// </summary>
        /// <param name="counter">Текущее значение счетчика</param>
        /// <param name="total">Итоговое значение счетчика</param>
        public CounterEventArgs(int counter, int total)
        {
            Counter = counter;
            Total = total;
        }

    }
}
