using Microsoft.Win32;
using PatientsFomsRepository.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PatientsFomsRepository.Views
{
    /// <summary>
    /// Логика взаимодействия для AdditionalSettingsView.xaml
    /// </summary>
    public partial class PatientsFileSettingsView : UserControl
    {
        public PatientsFileSettingsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog;

            if (CheckBoxDowloadNewFile.IsChecked == true)
                fileDialog = new SaveFileDialog();
            else
                fileDialog = new OpenFileDialog();

            if (string.IsNullOrEmpty(TextBoxFilePath.Text) == false)
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(TextBoxFilePath.Text);
                fileDialog.FileName = Path.GetFileName(TextBoxFilePath.Text);
            }

            fileDialog.Filter = "xlsx files (*.xslx)|*.xlsx";

            if (fileDialog.ShowDialog() == true)
            {
                var viewModel=(PatientsFileSettingsViewModel)DataContext;
                viewModel.Settings.PatientsFilePath= fileDialog.FileName;
            }
        }


    }
}
