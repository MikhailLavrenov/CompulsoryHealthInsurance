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
            /*
            var db = new UserContext();
            var patient = new Patient { Surname = "Бобров", Name = "Константин", Patronymic = "Игоревич", Initials = "БКИ", PatientId = 1 };

            db.Patients.Add(patient);
            db.SaveChanges();*/
            var pat = new Patient { Surname = "Бобров", Patronymic = "Игоревич", Initials = "БКИ", PatientId = 1 };
            var t= (pat.Name??"").Length;


            InitializeComponent();
        }
    }
}
