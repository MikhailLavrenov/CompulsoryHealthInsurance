using CHI.Application.Infrastructure;
using CHI.Application.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
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

        public DelegateCommand SaveSettingsCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }
        public DelegateCommand RestoreWindowCommand { get; }
        public DelegateCommand MaximizeWindowCommand { get; }
        public DelegateCommand MinimizeWindowCommand { get; }
        public DelegateCommand<Type> ShowViewCommand { get; }
        #endregion

        #region Конструкторы
        public ShellViewModel(IMainRegionService mainRegionService, ILicenseManager licenseManager, IDialogService dialogService)
        {
            if (!Settings.Instance.SuccessfulDecrypted)
            {
                var title = "Предупреждение";
                var message = $"У вас нет прав на расшифрование сохраненных логинов и паролей.{Environment.NewLine} Если продолжить сохраненные учетные записи будут утеряны. {Environment.NewLine}Продолжить?";
                var result = dialogService.ShowDialog(title, message);

                if (result == ButtonResult.OK)
                    System.Windows.Application.Current.Shutdown();
            }


            IsMaximizedWidow = System.Windows.Application.Current.MainWindow.WindowState == WindowState.Maximized ? true : false;
            MainRegionService = mainRegionService;
            ApplicationTitle = ((AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).First()).Title;

            SaveSettingsCommand = new DelegateCommand(() => Settings.Instance.Save());
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
