using Prism.Commands;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса MainRegion
    /// </summary>
    public interface IMainRegionService
    {
        string Status { get; set; }
        string Header { get; set; }
        bool IsBusy { get; set; }
        bool ShowStatus { get; }

        void SetCompleteStatus(string statusMessage);
        void SetBusyStatus(string statusMessage);
        void RequestNavigate(string targetName);

        DelegateCommand CloseStatusCommand { get; }
    }
}
