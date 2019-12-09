namespace CHI.Services
{
    /// <summary>
    /// Представляет учетные данные
    /// </summary>
    public interface ICredential
    {
        /// <summary>
        /// Логин
        /// </summary>
        string Login { get; }
        /// <summary>
        /// Пароль
        /// </summary>
        string Password { get; }
    }
}
