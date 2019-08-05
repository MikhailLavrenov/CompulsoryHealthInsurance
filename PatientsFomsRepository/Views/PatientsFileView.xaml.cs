using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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

            if (string.IsNullOrEmpty(TextBoxFilePath.Text))
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            else
                fileDialog.InitialDirectory = Path.GetDirectoryName(TextBoxFilePath.Text);

            fileDialog.Filter = "xlsx files (*.xslx)|*.xlsx";

            if (fileDialog.ShowDialog() == true)
                TextBoxFilePath.Text = fileDialog.FileName;
        }
    }
    }
