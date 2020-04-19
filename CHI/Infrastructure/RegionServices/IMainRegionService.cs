using Prism.Commands;
using Prism.Regions;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса MainRegion
    /// </summary>
    public interface IMainRegionService
    {
        string Message { get; set; }
        string Header { get; set; }
        bool IsLocked { get; set; }
        bool IsShowProgressBar { get; set; }
        bool IsShowDialog { get; set; }
        bool IsShowStatus { get; }
        bool CanNavigateBack { get; }


        void HideProgressBarWithhMessage(string statusMessage);
        void ShowProgressBarWithMessage(string statusMessage);
        void RequestNavigate(string targetName, bool canNavigateBack=false);
        void RequestNavigate(string targetName, NavigationParameters navigationParameters, bool canNavigateBack = false);
        void RequestNavigateBack();
        void ClearNavigationBack();

        DelegateCommand CloseStatusCommand { get; }
    }
}
