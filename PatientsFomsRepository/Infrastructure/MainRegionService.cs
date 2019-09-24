using Prism.Regions;

namespace PatientsFomsRepository.Infrastructure
{
    /// <summary>
    /// Сервис MainRegion
    /// </summary>
    public class MainRegionService : DomainObject, IMainRegionService
    {
        private string header;
        private string status;
        private bool isBusy;
        private IRegionManager regionManager;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value); }

        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
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
    }
}
