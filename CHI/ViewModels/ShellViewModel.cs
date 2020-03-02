using CHI.Infrastructure;
using CHI.Application.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace CHI.Application.ViewModels
{
    public class ShellViewModel : DomainObject
    {
        #region Поля
        private bool isMaximizedWindow;
        private IMainRegionService mainRegionService;
        #endregion

        #region Свойства 
        public IMainRegionService MainRegionService { get; set; }
        public string ApplicationTitle { get; }
        public bool ShowLicenseManager { get; }
        public bool IsMaximizedWidow { get => isMaximizedWindow; set => SetProperty(ref isMaximizedWindow, value); }

        public DelegateCommand SaveSettingsCommand { get; }
        public DelegateCommand CheckSettingsCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }
        public DelegateCommand RestoreWindowCommand { get; }
        public DelegateCommand MaximizeWindowCommand { get; }
        public DelegateCommand MinimizeWindowCommand { get; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IMainRegionService mainRegionService)
        {
            this.mainRegionService = mainRegionService;

            IsMaximizedWidow = System.Windows.Application.Current.MainWindow.WindowState == WindowState.Maximized ? true : false;
            MainRegionService = mainRegionService;
            ApplicationTitle = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First()).Title;

            SaveSettingsCommand = new DelegateCommand(() => Settings.Instance.Save());
            CheckSettingsCommand = new DelegateCommand(CheckSettingsExecute);
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
        private void CheckSettingsExecute()
        {
            if (Settings.Instance.FailedToDecrypt)
            {
                var message = $"Нет прав на доступ к учетным записям. Создана резервная копия настроек: {Settings.Instance.BackupSettingsFile}. Заново задайте учетные данные.";
                mainRegionService.SetCompleteStatus(message);
            }
        }
        #endregion
    }
}
