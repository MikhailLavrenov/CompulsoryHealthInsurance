﻿using CHI.Infrastructure;
using Prism.Mvvm;
using System.Collections.Generic;

namespace CHI.Models.ServiceAccounting
{
    public class Department : BindableBase, IHierarchical<Department>, IOrdered
    {
        string hexColor = "#FFFFFF";

        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string HexColor { get => hexColor; set => SetProperty(ref hexColor, value); }
        public List<Employee> Employees { get; set; }
        public List<Parameter> Parameters { get; set; }
        public bool IsRoot { get; set; }
        public Department Parent { get; set; }
        public List<Department> Childs { get; set; }
        public List<PlanningPermision> PlanningPermisions { get; set; }

        public Department()
        {
            Parameters = new List<Parameter>();
            Employees = new List<Employee>();
            PlanningPermisions = new List<PlanningPermision>();
            Childs = new List<Department>();
        }

        public Department(string name)
            : this()
        {
            Name = name;
        }
    }
}
