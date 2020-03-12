using Prism.Commands;
using Prism.Regions;

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
        bool CanNavigateBack { get; }


        void SetCompleteStatus(string statusMessage);
        void SetBusyStatus(string statusMessage);
        void RequestNavigate(string targetName, bool canNavigateBack=false);
        void RequestNavigate(string targetName, NavigationParameters navigationParameters, bool canNavigateBack = false);
        void RequestNavigateBack();

        DelegateCommand CloseStatusCommand { get; }
    }
}
