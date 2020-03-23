using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public enum ParameterKind
    {
        [Description("Пусто")] None=0,
        [Description("План")] EmployeePlan = 1,
        [Description("Факт")] EmployeeFact = 2,
        [Description("Отклонено")] EmployeeRejectedFact = 3,
        [Description("% Вып")] EmployeePercent = 4,
        [Description("План")] DepartmentCalculatedPlan = 5,
        [Description("План отд")] DepartmenHandPlan = 6,        
        [Description("Факт")] DepartmentFact = 7,
        [Description("Отклонено")] DepartmentRejectedFact = 8,
        [Description("% Вып")] DepartmentPercent = 9

    }
}
