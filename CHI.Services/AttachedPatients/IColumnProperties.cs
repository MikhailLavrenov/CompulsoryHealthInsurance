namespace CHI.Services.AttachedPatients
{
    public interface IColumnProperties
    {
        string Name { get; set; }
        string AltName { get; set; }
        bool Hide { get; set; }
        bool Delete { get; set; }
    }
}
