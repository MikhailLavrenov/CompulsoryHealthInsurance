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
        public List<UserDepartment> UserDepartments { get; set; }

        public User()
        {
            UserDepartments = new List<UserDepartment>();
        }
    }
}
