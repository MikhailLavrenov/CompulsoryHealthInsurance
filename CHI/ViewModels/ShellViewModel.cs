using CHI.Infrastructure;
using CHI.Models;
using Prism.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CHI.ViewModels
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
        public DelegateCommand<Type> NavigateCommand { get; }
        public DelegateCommand NavigateBackCommand { get; }
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
            NavigateCommand = new DelegateCommand<Type>(x => {  MainRegionService.RequestNavigate(x.Name); MainRegionService.ClearNavigationBack(); });
            NavigateBackCommand = new DelegateCommand(()=>MainRegionService.RequestNavigateBack());
            CloseWindowCommand = new DelegateCommand(CloseWindowExecute);
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
                mainRegionService.HideProgressBarWithhMessage(message);
            }
        }
        private void CloseWindowExecute()
        {
            //Необходимо чтобы срабатывал метод INavigationAwaire при закрытии приложения
            MainRegionService.RequestNavigate(string.Empty);
            System.Windows.Application.Current.Shutdown();
        }
        #endregion
    }
}
