using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WpfApp1.Models;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Test();


        }

        public static async void Test()
        {
            WebSiteSRZ site = new WebSiteSRZ("http://11.0.0.28/", true, "10.10.45.43", 3128);
            await site.Authorize("UshanovaTA", "UshanovaTA1");
            site.DbfToExcel(@"C:\Users\ЛавреновМВ\Desktop\ATT_MO_temp_ERW3sdcxf1XCa.DBF", @"C:\Users\ЛавреновМВ\Desktop\attmo.xlsx");
            //await site.GetPatientsExcelFile(DateTime.Now, @"C:\Users\ЛавреновМВ\Desktop\attmo.xlsx");
            site.Dispose();
        }
    }
}
