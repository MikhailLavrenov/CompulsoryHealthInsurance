using CHI.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Models.ServiceAccounting
{
    public class Report
    {
        public List<HeaderItem> RowHeaders { get; set; }
        public List<HeaderItem> ColumnHeaders { get; set; }
        public ValueItem[,] Values { get; set; }


        public Report(Department rootDepartment, Component rootComponent)
        {
            rootComponent.OrderChildsRecursive();
            var components = rootComponent.ToListRecursive()
                .Where(x => x.Indicators != null && x.Indicators.Count != 0)
                .ToList();

            var indicators = new List<Indicator>();

            foreach (var component in components)
                indicators.AddRange(component.Indicators.OrderBy(x => x.Order).ToList());

            ColumnHeaders = new List<HeaderItem>();



            rootDepartment.OrderChildsRecursive();
            var departments = rootDepartment.ToListRecursive()
                .Where(x => x.Employees != null && x.Employees.Count != 0)
                .ToList();

            var parameters = new List<Parameter>();

            foreach (var department in departments)
                foreach (var employee in department.Employees.OrderBy(x => x.Order))
                    parameters.AddRange(employee.Parameters.OrderBy(x=>x.Order).ToList());
            






            Values = new ValueItem[parameters.Count, indicators.Count];

            for (int row = 0; row < parameters.Count; row++)
                for (int col = 0; col < indicators.Count; col++)
                {
                    Values[row, col] = new ValueItem(row, col, parameters[row], indicators[col]);
                }
        }


        private static void CreateHeaderItem(Component component)
        {

        }

    }
}
