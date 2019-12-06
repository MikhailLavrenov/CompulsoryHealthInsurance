using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса MainRegion
    /// </summary>
    public interface IMainRegionService
    {
        string Status { get; set; }
        string Header { get; set; }
        bool IsBusy { get; set; }

        void SetCompleteStatus(string statusMessage);
        void SetBusyStatus(string statusMessage);
        void RequestNavigate(string targetName);
    }
}
