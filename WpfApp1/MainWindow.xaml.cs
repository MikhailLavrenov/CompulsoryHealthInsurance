using FomsPatientsDB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            /*
            var file = new PatientsFile();
            await file.Open(@"C:\Users\ЛавреновМВ\Desktop\myattmo.xlsx");
            var newPatients = await file.GetVerifedPatients();

            var db = new CacheDB();
            db.Patients.Load();
            var existenInsuaranceNumbers=db.Patients.Select(x => x.InsuranceNumber).ToHashSet();
            var newUniqPatients = newPatients
            .Where(x=>!existenInsuaranceNumbers.Contains(x.InsuranceNumber))
            .GroupBy(x=>x.InsuranceNumber)
            .Select(x=>x.First())
            .ToList();
            db.Patients.AddRange(newUniqPatients);
            db.SaveChanges();
            */
            var credentialList = new List<Credential>
                    {
                    new Credential() { Login = "SluzhenkoNV", Password = "SluzhenkoNV1", RequestsLimit=10 },
                    new Credential() { Login = "BershadskayaNP", Password = "BershadskayaNP1", RequestsLimit=10 }
                    };
            /*
            var credentials = new  RoundRobinCredentials(credentialList);            
            Credential credential;
            for (int i = 0; i < 10; i++)
                {
                var flag=credentials.TryGetNext(out credential);
                }
 */
           
            var file = new PatientsFile();
            await file.Open(@"C:\Users\ЛавреновМВ\Desktop\attmo.xlsx");
            var unverifiedPatients = await file.GetUnverifiedInsuaranceNumbersAsync(20);
            var verifiedPatients = WebSiteSRZ.GetPatients("http://11.0.0.28/", "10.10.45.43",  3128, unverifiedPatients, credentialList, 2);


            /*
            WebSiteSRZ site = new WebSiteSRZ("http://11.0.0.28/", "10.10.45.43", 3128);
            site.Authorize(new Credential() { Login = "UshanovaTA", Password = "UshanovaTA1" });
            
            //await site.GetPatientsFile(@"C:\Users\ЛавреновМВ\Desktop\attmo.xlsx", DateTime.Now);
            var patient= await site.GetPatient("2757010827000340");
            site.Dispose();
            */
        }
    }
}
