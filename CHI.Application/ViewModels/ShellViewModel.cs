using CHI.Application.Infrastructure;
using Prism.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CHI.Application.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        public bool isMaximizedWindow;
        #endregion

        #region Свойства 
        public IMainRegionService MainRegionService { get; set; }
        public string ApplicationTitle { get; }
        public bool ShowLicenseManager { get; }
        public bool IsMaximizedWidow { get => isMaximizedWindow; set => SetProperty(ref isMaximizedWindow, value); }


        public DelegateCommand CloseWindowCommand { get; }
        public DelegateCommand RestoreWindowCommand { get; }
        public DelegateCommand MaximizeWindowCommand { get; }
        public DelegateCommand MinimizeWindowCommand { get; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IMainRegionService mainRegionService, ILicenseManager licenseManager)
        {
            IsMaximizedWidow = System.Windows.Application.Current.MainWindow.WindowState == WindowState.Maximized ? true : false;
            MainRegionService = mainRegionService;
            ApplicationTitle = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First()).Title;

            ShowViewCommand = new DelegateCommand<Type>(x => MainRegionService.RequestNavigate(x.Name));
            CloseWindowCommand = new DelegateCommand(() => System.Windows.Application.Current.Shutdown());
            RestoreWindowCommand = new DelegateCommand(RestoreWindowExecute);
            MaximizeWindowCommand = new DelegateCommand(MaximizeWindowExecute);
            MinimizeWindowCommand = new DelegateCommand(() => System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized);
        }
        #endregion

        #region Методы
        private void RestoreWindowExecute()
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
            IsMaximizedWidow = false;
        }
        private void MaximizeWindowExecute()
        {
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Maximized;
            IsMaximizedWidow = true;
        }
        #endregion
    }
}
