using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class User:BindableBase
    {

        public int Id { get; set; }
        public string Sid { get; set; }
        public string Name { get; set; }
        public List<PlanningPermision> PlanningPermisions { get; set; }
        public bool RegistersPermision { get; set; }
        public bool ReferencesPerimision { get; set; }
        public bool UsersPerimision { get; set; }
        public bool SettingsPermision { get; set; }
        public bool AttachedPatientsPermision { get; set; }
        public bool MedicalExaminationsPermision { get; set; }


        public User()
        {
            PlanningPermisions = new List<PlanningPermision>();
        }
    }
}
