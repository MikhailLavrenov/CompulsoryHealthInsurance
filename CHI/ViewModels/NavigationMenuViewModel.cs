using CHI.Infrastructure;
using CHI.Models;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CHI.ViewModels
{
    public class NavigationMenuViewModel : DomainObject, IRegionMemberLifetime
    {
        IMainRegionService mainRegionService;

        public bool KeepAlive { get => false; }

        public DelegateCommand<object> SwitchViewCommand { get; }

        public NavigationMenuViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Меню";

            SwitchViewCommand = new DelegateCommand<object>(SwitchViewExecute);
        }

        private void SwitchViewExecute(object view)
        {
            string name;

            if (view is Type)
                name = ((Type)view).Name;
            else if (view is string)
                name = (string)view;
            else
                name = string.Empty;


            if (name == "PlanReportView")
            {
                name = "ReportView";
                var parameters = new NavigationParameters();
                parameters.Add("IsPlanMode", true);

                mainRegionService.RequestNavigate(name, parameters);
            }
            else
                mainRegionService.RequestNavigate(name);
            
        }
    }
}
