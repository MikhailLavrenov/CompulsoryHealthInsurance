using Microsoft.Win32;
using System.IO;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Сервис файлового диалога
    /// </summary>
    class FileDialogService : IFileDialogService
    {
        public FileDialogType DialogType { get; set; }
        public string Filter { get; set; }
        public string FullPath { get; set; }

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
                fileDialog = new OpenFileDialog();

            fileDialog.Filter = Filter;

            if (!string.IsNullOrEmpty(FullPath))
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(FullPath);
                fileDialog.FileName = Path.GetFileName(FullPath);
            }
           
            var result = fileDialog.ShowDialog();
            FullPath = fileDialog.FileName;

            return result;
        }
    }
}
