using Microsoft.Win32;
using System.IO;

namespace PatientsFomsRepository.Infrastructure
{
    class FileDialogService : IFileDialogService
    {
        public DialogType DialogType { get; set; }
        public string Filter { get; set; }
        public string FullPath { get; set; }

        public bool? ShowDialog()
        {
            FileDialog fileDialog = null;            

            if (DialogType == DialogType.Save)
                fileDialog = new SaveFileDialog();
            else if (DialogType == DialogType.Open)
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
