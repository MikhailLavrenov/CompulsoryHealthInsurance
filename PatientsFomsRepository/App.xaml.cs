using PatientsFomsRepository.ViewModels;
using PatientsFomsRepository.Views;
using System.Windows;

namespace PatientsFomsRepository
    {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
        {
        protected override void OnStartup(StartupEventArgs e)
            {
            base.OnStartup(e);
            var view = new MainWindowView();
            var viewModel = new MainWindowViewModel();
            view.DataContext = viewModel;
            view.Show();
            }
        }
    }
