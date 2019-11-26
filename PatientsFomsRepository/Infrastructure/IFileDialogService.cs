using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса файлового диалога
    /// </summary>
    public interface IFileDialogService
    {
        FileDialogType DialogType { get; set; }
        string Filter { get; set; }
        string FullPath { get; set; }
        bool? ShowDialog();
    }
}
