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
        private bool inProgress;
        private IRegionManager regionManager;

        public string Header { get => header; set => SetProperty(ref header, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public bool InProgress { get => inProgress; set => SetProperty(ref inProgress, value); }

        public MainRegionService(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void SetCompleteStatus(string statusMessage)
        {
            if (InProgress)
                Status = $"Завершено. {statusMessage}";
            else
                Status = statusMessage;
            InProgress = false;
        }
        public void SetInProgressStatus(string statusMessage)
        {
            Status = $"Ожидайте... {statusMessage}";
            InProgress = true;
        }
        public void RequestNavigate(string targetName)
        {
            InProgress = false;
            Status = string.Empty;
            regionManager.RequestNavigate(RegionNames.MainRegion, targetName);
        }
    }
}
