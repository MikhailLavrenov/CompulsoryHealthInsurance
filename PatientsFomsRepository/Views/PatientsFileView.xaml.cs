using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PatientsFomsRepository.Views
{
    /// <summary>
    /// Логика взаимодействия для AttachedPatientsView.xaml
    /// </summary>
    public partial class PatientsFileView : UserControl
    {
        public PatientsFileView()
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
                TextBoxFilePath.Text = fileDialog.FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
