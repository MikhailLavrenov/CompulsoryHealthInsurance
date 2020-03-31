using System.ComponentModel.DataAnnotations.Schema;

namespace CHI.Models.ServiceAccounting
{
    public class ValueItem
    {
        public int Id { get; set; }
        public Parameter Parameter { get; set; }
        public Indicator Indicator { get; set; }
        public double Value { get; set; }
        [NotMapped] public int Row { get; set; }
        [NotMapped] public int Column { get; set; }
        [NotMapped] public int Priority { get; set; }

        public ValueItem(int row, int column, Parameter parameter, Indicator indicator)
        {
            Row = row;
            Column = column;
            Parameter = parameter;
            Indicator = indicator;

            var rowPriority = parameter.Employee != null ? 1 : 0;

            if (parameter.Kind != ParameterKind.DepartmentPercent && parameter.Kind != ParameterKind.EmployeePercent)
                rowPriority++;

            var department = parameter.Department != null ? parameter.Department : parameter.Employee.Department;

            while (!department.IsRoot)
            {
                rowPriority++;
                department = department.Parent;
            };

            var columnPriority = 0;

            var component = indicator.Component;

            while(!component.IsRoot)
            {
                columnPriority++;
                component = component.Parent;
            };

            Priority = rowPriority * columnPriority;
        }
    }
}
