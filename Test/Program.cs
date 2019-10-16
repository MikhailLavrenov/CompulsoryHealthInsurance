using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHI.Modules.MedicalExaminations.Services;

namespace CHI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = @"C:\Users\ЛавреновМВ\Desktop\ExportFile_2019_10_10_15_59_11(2019_09_01 2019_09_30).zip";

            var register = new BillsRegister(fileName);
            var examinationNames = new string[] { @"DPM", @"DVM", @"DOM" };
            var patientsNames= new string[] { @"LPM", @"LVM", @"LOM" };

            var patients = register.GetPatientsExaminations(examinationNames, patientsNames);
            var t = patients.Where(x => x.Examinations.Count > 1).ToList();
        }
    }
}
