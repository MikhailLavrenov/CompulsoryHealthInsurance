namespace CHI.Services.AttachedPatients
{
    /// <summary>
    /// Представляет настраиваемые свойства столбца файла выгрузки.
    /// </summary>
    public interface IColumnProperties
    {
        /// <summary>
        /// Оригинальное название столбца
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Понятное название столбца
        /// </summary>
        string AltName { get; set; }
        /// <summary>
        /// Скрыть
        /// </summary>
        bool Hide { get; set; }
        /// <summary>
        /// Удалить
        /// </summary>
        bool Delete { get; set; }
    }
}
