using CHI.Application.Infrastructure;
using CHI.Application.ViewModels;
using CHI.Application.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Threading;

namespace CHI.Licensing
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<LicenseAdminView>();
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IFileDialogService, FileDialogService>();
        }
    }

}
