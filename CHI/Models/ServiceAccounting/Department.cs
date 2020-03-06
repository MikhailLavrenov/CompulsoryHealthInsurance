namespace CHI.Models.ServiceAccounting
{
    public class Department
    {
        public static string UnknownTitle { get; } = "Неизвестно";

        public int Id { get; set; }
        public string Title { get; set; }

        public Department(string title)
        {
            Title = title;
        }
    }
}
