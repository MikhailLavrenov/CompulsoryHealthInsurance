using CHI.Application.Infrastructure;
using Prism.Commands;
using System;
using System.Linq;
using System.Reflection;

namespace CHI.Application.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        #endregion

        #region Свойства 
        public IMainRegionService MainRegionService { get; set; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        public string ApplicationTitle { get; }
        public bool ShowLicenseManager { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IMainRegionService mainRegionService, ILicenseManager licenseManager)
        {
             MainRegionService = mainRegionService;
            ApplicationTitle = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First()).Title;

            ShowViewCommand = new DelegateCommand<Type>(x => MainRegionService.RequestNavigate(x.Name));
        }
        #endregion

        #region Методы
        #endregion
    }
}
