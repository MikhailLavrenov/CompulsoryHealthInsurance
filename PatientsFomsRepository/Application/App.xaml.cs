using PatientsFomsRepository.Infrastructure;
using PatientsFomsRepository.ViewModels;
using PatientsFomsRepository.Views;
using System.Windows;
using System.Windows.Controls;

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

            //для редактирования ячеек datagrid одним кликом
            EventManager.RegisterClassHandler(typeof(DataGrid), DataGrid.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(DataGridHelper.DataGridPreviewMouseLeftButtonDownEvent));

            var view = new MainWindowView();
            view.DataContext = new MainWindowViewModel();
            view.Show();
        }
    }
}
