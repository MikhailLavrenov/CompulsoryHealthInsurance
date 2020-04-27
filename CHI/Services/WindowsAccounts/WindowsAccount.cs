namespace CHI.Services.WindowsAccounts
{
    /// <summary>
    /// Представляет информацию об учетной записи пользователя Windows
    /// </summary>
    public class WindowsAccount
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Sid { get; set; }      
        
        public WindowsAccount(string name, string login,  string sid )
        {
            Sid = sid;
            Login = login;
            Name = name;
        }
    }

}
