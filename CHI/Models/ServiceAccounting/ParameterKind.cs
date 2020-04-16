using CHI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public enum ParameterKind
    {
        [MultipleDescription("Выберите значение", "Не задано")] None=0,
        [MultipleDescription("План", "План")] EmployeePlan = 1,
        [MultipleDescription("Факт", "Факт")] EmployeeFact = 2,
        [MultipleDescription("Отклонено", "Откл")] EmployeeRejectedFact = 3,
        [MultipleDescription("% Выполнения", "% Вып")] EmployeePercent = 4,
        [MultipleDescription("План", "План")] DepartmentCalculatedPlan = 5,
        [MultipleDescription("План отд", "План отд")] DepartmentHandPlan = 6,        
        [MultipleDescription("Факт", "Факт")] DepartmentFact = 7,
        [MultipleDescription("Отклонено", "Откл")] DepartmentRejectedFact = 8,
        [MultipleDescription("% Выполнения", "% Вып")] DepartmentPercent = 9

    }
}
