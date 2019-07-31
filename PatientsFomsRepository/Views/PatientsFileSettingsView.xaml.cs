using Microsoft.Win32;
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

            if (string.IsNullOrEmpty(TextBoxFilePath.Text))
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            else
                fileDialog.InitialDirectory = Path.GetDirectoryName(TextBoxFilePath.Text);

            fileDialog.Filter = "xlsx files (*.xslx)|*.xlsx";

            if (fileDialog.ShowDialog()==true)
            {
                TextBoxFilePath.Text = fileDialog.FileName;
            }

        }
    }
}
