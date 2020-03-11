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

        public DelegateCommand<Type> SwitchViewCommand { get; }

        public NavigationMenuViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            mainRegionService.Header = "Меню";

            SwitchViewCommand = new DelegateCommand<Type>(x => mainRegionService.RequestNavigate(x.Name));
        }

    }
}
