using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CHI.Models.ServiceAccounting
{
    public class Employee : BindableBase, ICloneable
    {
        Department department;

        public int Id { get; set; }
        public Medic Medic { get; set; }
        public Specialty Specialty { get; set; }
        public Department Department { get => department; set => SetProperty(ref department, value); }
        public AgeKind AgeKind { get; set; }
        public bool IsArchive { get; set; }
        public int Order { get; set; }
        public List<Parameter> Parameters { get; set; }


        public Employee()
        {
            Parameters = new List<Parameter>();
        }

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

        public object Clone()
        {
            var clone = (Employee)MemberwiseClone();
            clone.Id = 0;
            clone.Parameters = new List<Parameter>();

            foreach (var parameter in Parameters)
                clone.Parameters.Add((Parameter)parameter.Clone());

            return clone;
        }
    }
}
