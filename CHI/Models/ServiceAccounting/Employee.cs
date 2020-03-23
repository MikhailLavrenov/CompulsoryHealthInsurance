using Prism.Mvvm;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class Employee : BindableBase
    {
        Department department;

        public int Id { get; set; }
        public Medic Medic { get; set; }
        public Specialty Specialty { get; set; }
        public Department Department { get => department; set => SetProperty(ref department, value); }
        public int Order { get; set; }
        public List<Parameter> Parameters { get; set; }

        /// <summary>
        /// Создает штатную единицу с заданными ФОМС кодами и неопределенными данными
        /// </summary>
        /// <param name="medicFomsId"></param>
        /// <param name="specialtyFomsId"></param>
        public static Employee CreateUnknown(string medicFomsId, int specialtyFomsId)
        {
            return new Employee
            {
                Medic = Medic.CreateUnknown(medicFomsId),
                Specialty = Specialty.CreateUnknown(specialtyFomsId)
            };
        }
    }
}
