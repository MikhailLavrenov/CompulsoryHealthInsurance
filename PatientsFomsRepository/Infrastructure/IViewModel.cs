using Prism.Regions;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Интерфейс для всех ViewModel
    /// </summary>
    public interface IViewModel : IRegionMemberLifetime
    {
        #region Properties
        /// <summary>
        /// Название представления для кнопки/ссылки
        /// </summary>
        string ShortCaption { get; }
        /// <summary>
        /// Название представления для заголовка
        /// </summary>
        string FullCaption { get; }
        /// <summary>
        /// Сообщения о ходе выполнения
        /// </summary>
        string Progress { get; }
        #endregion
    }
}
