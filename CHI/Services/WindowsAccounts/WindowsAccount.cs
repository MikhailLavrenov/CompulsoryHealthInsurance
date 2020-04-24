namespace CHI.Services.WindowsAccounts
{
    /// <summary>
    /// Представляет информацию об учетной записи пользователя Windows
    /// </summary>
    public class WindowsAccount
    {
        public string Sid { get; set; }
        public string Name { get; set; }

        public WindowsAccount(string sid, string name)
        {
            Sid = sid;
            Name = name;
        }
    }

}
