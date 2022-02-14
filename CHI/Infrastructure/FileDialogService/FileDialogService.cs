using Microsoft.Win32;
using System.IO;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Сервис файлового диалога
    /// </summary>
    public class FileDialogService : IFileDialogService
    {
        public FileDialogType DialogType { get; set; }
        public string Filter { get; set; }
        public bool MiltiSelect { get; set; }
        public string[] FileNames { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// Показать диалоговое окно модально
        /// </summary>
        /// <returns></returns>
        public bool? ShowDialog()
        {
            FileDialog fileDialog = null;

            if (DialogType == FileDialogType.Save)
                fileDialog = new SaveFileDialog();
            else if (DialogType == FileDialogType.Open)
                fileDialog = new OpenFileDialog() { Multiselect = MiltiSelect };

            fileDialog.Filter = Filter;


            if (!string.IsNullOrEmpty(FileName))
            {
                var initialDirectory = Path.GetDirectoryName(FileName);
                if (Directory.Exists(initialDirectory))
                    fileDialog.InitialDirectory = initialDirectory;

                fileDialog.FileName = Path.GetFileName(FileName);
            }

            var result = fileDialog.ShowDialog();

            FileName = fileDialog.FileName;
            FileNames = fileDialog.FileNames;

            return result;
        }
    }
}
