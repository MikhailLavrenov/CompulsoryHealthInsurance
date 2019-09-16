using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
{
    public interface IFileDialogService
    {
        DialogType DialogType { get; set; }
        string Filter { get; set; }
        string FullPath { get; set; }
        bool? ShowDialog();
    }
}
