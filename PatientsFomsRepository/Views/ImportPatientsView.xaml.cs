using Microsoft.Win32;
using PatientsFomsRepository.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;


namespace PatientsFomsRepository.Views
{
    /// <summary>
    /// Логика взаимодействия для ImportPatientsView.xaml
    /// </summary>
    public partial class ImportPatientsView : UserControl
    {
        public ImportPatientsView()
        {
            InitializeComponent();
        }
        private void ButtonImportPatientsClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "xlsx files (*.xslx)|*.xlsx";

            if (fileDialog.ShowDialog() == true)
            {
                var viewModel = (ImportPatientsViewModel)DataContext;
                viewModel.ImportFilePath = fileDialog.FileName;
            }
        }
        private void ButtonSaveExampleClick(object sender, RoutedEventArgs e)
        {            
            var fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "xlsx files (*.xslx)|*.xlsx";
            fileDialog.FileName = "Пример для загрузки ФИО";

            if (fileDialog.ShowDialog() == true)
            {                
                var viewModel = (ImportPatientsViewModel)DataContext;
                viewModel.SaveExampleFilePath = fileDialog.FileName;
            }
        }
    }
}
