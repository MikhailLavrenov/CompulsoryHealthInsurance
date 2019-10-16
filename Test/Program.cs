using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHI.Modules.MedicalExaminations.Services;
using PatientsFomsRepository.Models;

namespace CHI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestWebServer();
        }
        static void TestWebServer()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var web = new WebServer(url, proxyServer, proxyPort);
            var cred = new Credential() { Login= "UshanovaTA", Password= "UshanovaTA1" };
            web.TryAuthorize(cred);
        }
        static void TestBillsRegisters()
        {
            var fileName = @"C:\Users\ЛавреновМВ\Desktop\ExportFile_2019_10_10_15_59_11(2019_09_01 2019_09_30).zip";

            var register = new BillsRegister(fileName);
            var examinationNames = new string[] { @"DPM", @"DVM", @"DOM" };
            var patientsNames = new string[] { @"LPM", @"LVM", @"LOM" };

            var patients = register.GetPatientsExaminations(examinationNames, patientsNames);
            var t = patients.Where(x => x.Examinations.Count > 1).ToList();
        }
    }
}
