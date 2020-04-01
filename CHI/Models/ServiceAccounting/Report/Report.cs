using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Report
    {
        RowHeaderGroup rootRowHeader;
        ColumnHeaderGroup rootColumnHeader;
        int rowsCount;
        int columnsCount;
        int maxRowPriority;
        int maxColumnPriority;

        public List<RowHeaderItem> RowHeaders { get; private set; }
        public List<ColumnHeaderItem> ColumnHeaders { get; private set; }
        public ValueItem[,] Values { get; private set; }
        public List<ValueItem> ValuesList { get; private set; }

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

            rootColumnHeader = ColumnHeaderGroup.CreateHeadersRecursive(null, rootComponent);

            ColumnHeaders = new List<ColumnHeaderItem>();
            foreach (var header in rootColumnHeader.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                ColumnHeaders.AddRange(header.HeaderItems);

            maxColumnPriority = ColumnHeaders.Max(x => x.Priority);


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

            rootRowHeader = RowHeaderGroup.CreateHeadersRecursive(null, rootDepartment);

            RowHeaders = new List<RowHeaderItem>();
            foreach (var header in rootRowHeader.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                RowHeaders.AddRange(header.HeaderItems);

            maxRowPriority = RowHeaders.Max(x => x.Priority);

            Values = new ValueItem[parameters.Count, indicators.Count];

            rowsCount = parameters.Count;
            columnsCount = indicators.Count;

            for (int row = 0; row < RowHeaders.Count; row++)
                for (int col = 0; col < ColumnHeaders.Count; col++)
                    Values[row, col] = new ValueItem(row, col, RowHeaders[row], ColumnHeaders[col]);

            ValuesList = Values.Cast<ValueItem>().ToList();
        }

        public void Build(List<Case> cases)
        {
            RowHeaders
                .Where(x => x.Group.Employee != null)
                .Select(x => x.Group)
                .Distinct()
                .ToList()
                .ForEach(x => x.FactCases = cases.Where(y => y.Employee == x.Employee).ToList());

            for (int rowPriority = maxRowPriority; rowPriority <= 0; rowPriority--)
                for (int columnPriority = maxColumnPriority; columnPriority <= 0; columnPriority--)
                    foreach (var valueItem in ValuesList.Where(x => x.RowHeader.Priority == rowPriority && x.ColumnHeader.Priority == columnPriority))
                    {




                    }
            {











            }








        }

    }
}
