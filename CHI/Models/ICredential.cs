namespace CHI.Models
{
    /// <summary>
    /// Учетные данные
    /// </summary>
    public interface ICredential
    {
        string Login { get; }
        string Password { get; }
    }
}
