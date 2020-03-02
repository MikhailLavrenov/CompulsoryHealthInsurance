namespace CHI.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса файлового диалога
    /// </summary>
    public interface IFileDialogService
    {
        FileDialogType DialogType { get; set; }
        string Filter { get; set; }
        bool MiltiSelect { get; set; }
        string[] FileNames { get; set; }
        string FileName { get; set; }
        bool? ShowDialog();
    }
}
