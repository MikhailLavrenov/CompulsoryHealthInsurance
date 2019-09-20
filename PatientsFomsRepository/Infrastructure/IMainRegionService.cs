using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса MainRegion
    /// </summary>
    public interface IMainRegionService
    {
        string Status { get; set; }
        string Header { get; set; }
        bool InProgress { get; set; }

        void SetCompleteStatus(string statusMessage);
        void SetInProgressStatus(string statusMessage);
        void RequestNavigate(string targetName);
    }
}
