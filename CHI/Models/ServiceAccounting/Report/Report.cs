using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Report
    {
        HeaderGroup rootRowHeader;
        HeaderGroup rootColumnHeader;
        int rowsCount;
        int columnsCount;
        int maxPriority;

        public List<HeaderItem> RowHeaders { get; private set; }
        public List<HeaderItem> ColumnHeaders { get; private set; }
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

            rowsCount = parameters.Count;
            columnsCount = indicators.Count;
            maxPriority = 0;

            for (int row = 0; row < parameters.Count; row++)
                for (int col = 0; col < indicators.Count; col++)
                {
                    Values[row, col] = new ValueItem(row, col, parameters[row], indicators[col]);

                    if (Values[row, col].Priority > maxPriority)
                        maxPriority = Values[row, col].Priority;
                }

            ValuesList = Values.Cast<ValueItem>().ToList();
        }

        public void Build(List<Case> cases)
        {
            var employeesFactDataSource = new Dictionary<Employee, List<Case>>();

            Enumerable.Range(0, Values.GetLength(0))
                .Select(x => Values[x, 0].Parameter)
                .Where(x => x.Kind == ParameterKind.EmployeeFact)
                .Select(x => x.Employee)
                .ToList()
                .ForEach(x => employeesFactDataSource.Add(x, cases.Where(y => y.Employee == x).ToList()));


            for (int priority = maxPriority; priority <= 0; priority--)
                foreach (var valueItem in ValuesList.Where(x => x.Priority == priority))
                {
                    var parameter = valueItem.Parameter;
                    var indicator= valueItem.Indicator;
                    var component = valueItem.Indicator.Component;


                    if (valueItem.Parameter.Kind == ParameterKind.EmployeeFact)
                    {
                        if (valueItem.Indicator.Component.CaseFilters.First().Kind == CaseFilterKind.Total)
                        {

                        }
                        else
                        {





                        }











                    }




                }



        }

    }
}
