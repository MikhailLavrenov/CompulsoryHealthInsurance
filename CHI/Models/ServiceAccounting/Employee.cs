using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Employee
    {
        public int Id { get; set; }
        public Medic Medic { get; set; }
        public Specialty Specialty { get; set; }
        public Department Department { get; set; }

        public Employee()
        {
        }

        public Employee(string medicFomsId, int specialtyFomsId)
        {
            Medic = new Medic();
            Medic.FomsId = medicFomsId;
            Specialty = new Specialty();
            Specialty.FomsId = specialtyFomsId;
        }
    }
}
