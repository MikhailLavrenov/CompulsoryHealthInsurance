using CHI.Application.Views;
using Prism.Commands;
using Prism.Regions;
using System.Windows;

namespace CHI.Application.Infrastructure
{
    /// <summary>
    /// Сервис MainRegion
    /// </summary>
    public class MainRegionService : DomainObject, IMainRegionService
    {
        private string header;
        private string status;
        private bool isBusy;
        private bool showStatus;
        private IRegionManager regionManager;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Status
        {
            get => status;
            set
            {
                SetProperty(ref status, value);
                ShowStatus = !string.IsNullOrEmpty(value);
            }
        }
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, SwitchProgressBar); }
        public bool ShowStatus { get => showStatus; private set => SetProperty(ref showStatus, value); }

        public DelegateCommand CloseStatusCommand { get; }

        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            CloseStatusCommand = new DelegateCommand(CloseStatusExecute);
        }

        public void SetCompleteStatus(string statusMessage)
        {
            Status = statusMessage;
            IsBusy = false;
        }
        public void SetBusyStatus(string statusMessage)
        {
            Status = $"{statusMessage}";
            IsBusy = true;
        }
        public void RequestNavigate(string targetName)
        {
            IsBusy = false;
            Status = string.Empty;
            regionManager.RequestNavigate(RegionNames.MainRegion, targetName);
        }
        private void SwitchProgressBar()
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (IsBusy)
                    regionManager.RequestNavigate(RegionNames.ProgressBarRegion, nameof(ProgressBarView));
                else
                    regionManager.Regions[RegionNames.ProgressBarRegion].RemoveAll();
            });
        }
        private void CloseStatusExecute()
        {
            Status = string.Empty;
        }
    }
}
