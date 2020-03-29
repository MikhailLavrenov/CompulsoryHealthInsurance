using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Report
    {
        HeaderGroup rootRowHeader;
        HeaderGroup rootColumnHeader;

        public List<HeaderItem> RowHeaders { get; private set; }
        public List<HeaderItem> ColumnHeaders { get; private set; }
        public ValueItem[,] Values { get; private set; }

        public Report(Department rootDepartment, Component rootComponent)
        {
            rootComponent.OrderChildsRecursive();
            var components = rootComponent.ToListRecursive()
                .Where(x => x.Indicators?.Any() ?? false)
                .ToList();

            var indicators = new List<Indicator>();

            foreach (var component in components)
            {
                component.Indicators = component.Indicators.OrderBy(x => x.Order).ToList();

                indicators.AddRange(component.Indicators);
            }

            rootColumnHeader = HeaderGroup.CreateHeadersRecursive(null, rootComponent);

            ColumnHeaders = new List<HeaderItem>();
            foreach (var header in rootColumnHeader.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                ColumnHeaders.AddRange(header.HeaderItems);


            rootDepartment.OrderChildsRecursive();
            var departments = rootDepartment.ToListRecursive()
                .Where(x => x.Employees?.Any() ?? false)
                .ToList();

            var parameters = new List<Parameter>();

            foreach (var department in departments)
            {
                department.Employees = department.Employees.OrderBy(x => x.Order).ToList();
                department.Parameters = department.Parameters.OrderBy(x => x.Order).ToList();

                foreach (var employee in department.Employees)
                {
                    employee.Parameters = employee.Parameters.OrderBy(x => x.Order).ToList();

                    parameters.AddRange(employee.Parameters);
                }
            }

            rootRowHeader = HeaderGroup.CreateHeadersRecursive(null, rootDepartment);

            RowHeaders = new List<HeaderItem>();
            foreach (var header in rootRowHeader.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                RowHeaders.AddRange(header.HeaderItems);


            Values = new ValueItem[parameters.Count, indicators.Count];

            for (int row = 0; row < parameters.Count; row++)
                for (int col = 0; col < indicators.Count; col++)
                    Values[row, col] = new ValueItem(row, col, parameters[row], indicators[col]);
        }

        public void Build(List<Case> cases)
        {

        }

    }
}
