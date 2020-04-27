using Prism.Commands;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    /// <summary>
    /// Интерфейс сервиса MainRegion
    /// </summary>
    public interface IMainRegionService
    {
        string Message { get; set; }
        string Header { get; set; }
        bool IsLocked { get; }
        bool IsShowProgressBar { get; set; }
        bool IsShowDialog { get; set; }
        bool IsShowStatus { get; }
        bool CanNavigateBack { get; }

        DelegateCommand CloseStatusCommand { get; }


        void HideProgressBar(string statusMessage);
        void ShowProgressBar(string statusMessage);
        void RequestNavigate(string targetName, bool canNavigateBack = false);
        void RequestNavigate(string targetName, NavigationParameters navigationParameters, bool canNavigateBack = false);
        void RequestNavigateHome();
        void RequestNavigateBack();        
        void ClearNavigationBack();
        Task<bool> ShowNotificationDialog(string message);
        Task<Color> ShowColorDialog(Color defaultColor);



    }
}
