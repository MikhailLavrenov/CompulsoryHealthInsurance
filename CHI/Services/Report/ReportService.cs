using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class ReportService
    {
        RowHeaderGroup rootRow;
        ColumnHeaderGroup rootColumn;
        int rowsCount;
        int columnsCount;
        int maxRowPriority;
        int maxColumnPriority;

        public List<RowHeaderItem> Rows { get; private set; }
        public List<ColumnHeaderItem> Columns { get; private set; }
        public ValueItem[,] Values { get; private set; }


        public ReportService(Department rootDepartment, Component rootComponent)
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

            rootColumn = ColumnHeaderGroup.CreateHeadersRecursive(null, rootComponent);

            Columns = new List<ColumnHeaderItem>();
            foreach (var header in rootColumn.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                Columns.AddRange(header.HeaderItems);

            Enumerable.Range(0, Columns.Count).ToList().ForEach(x => Columns[x].Index = x);

            maxColumnPriority = Columns.Max(x => x.Priority);


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

            rootRow = RowHeaderGroup.CreateHeadersRecursive(null, rootDepartment);

            Rows = new List<RowHeaderItem>();
            foreach (var header in rootRow.ToListRecursive().Skip(1).Where(x => x.HeaderItems?.Any() ?? false))
                Rows.AddRange(header.HeaderItems);

            Enumerable.Range(0, Rows.Count).ToList().ForEach(x => Rows[x].Index = x);

            maxRowPriority = Rows.Max(x => x.Priority);

            Values = new ValueItem[parameters.Count, indicators.Count];

            rowsCount = parameters.Count;
            columnsCount = indicators.Count;

            for (int row = 0; row < Rows.Count; row++)
                for (int col = 0; col < Columns.Count; col++)
                    Values[row, col] = new ValueItem(row, col, Rows[row], Columns[col]);
        }


        public void Build(List<Case> factCases, List<Plan> plans, List<ServiceClassifier> classifiers)
        {
            //заполняет факт по оказанным случаям
            foreach (var row in Rows.Where(x => x.Parameter.Kind == ParameterKind.EmployeeFact))
            {
                var rowCases = factCases.Where(x => x.Employee == row.Parameter.Employee).ToList();

                ColumnHeaderGroup lastGroup = null;
                IEnumerable<Case> selectedCases = null;

                foreach (var column in Columns.Where(x => x.Group.CaseFilters.First().Kind != CaseFilterKind.Total))
                {
                    var valueItem = Values[row.Index, column.Index];

                    if (lastGroup == null || lastGroup != valueItem.ColumnHeader.Group)
                    {
                        var filterGroups = column.Group.Component.CaseFilters
                            .GroupBy(x => x.Kind)
                            .Select(x => new { x.Key, Codes = x.Select(y => y.Code).ToList() });

                        var treatmentCodes = filterGroups.FirstOrDefault(x => x.Key == CaseFilterKind.TreatmentPurpose)?.Codes;
                        var visitCodes = filterGroups.FirstOrDefault(x => x.Key == CaseFilterKind.VisitPurpose)?.Codes;
                        var containsServiceCodes = filterGroups.FirstOrDefault(x => x.Key == CaseFilterKind.ContainsService)?.Codes;
                        var notContainsServiceCodes = filterGroups.FirstOrDefault(x => x.Key == CaseFilterKind.NotContainsService)?.Codes;

                        //IEnumerable<Case> query= factCases;
                        //selectedCases = new List<Case>();

                        selectedCases = factCases.Where(x => x.PaidStatus != PaidKind.Refuse);

                        if (treatmentCodes?.Any() ?? false)
                            selectedCases = selectedCases.Join(treatmentCodes, mCase => mCase.VisitPurpose, code => code, (mCase, code) => mCase);
                        if (visitCodes?.Any() ?? false)
                            selectedCases = selectedCases.Join(visitCodes, mCase => mCase.VisitPurpose, code => code, (mCase, code) => mCase);
                        if (containsServiceCodes?.Any() ?? false)
                            selectedCases = selectedCases.Where(x => x.Services.Any(y => containsServiceCodes.Contains(y.Code)));
                        if (notContainsServiceCodes?.Any() ?? false)
                            selectedCases = selectedCases.Where(x => x.Services.Any(y => !containsServiceCodes.Contains(y.Code)));

                        selectedCases = selectedCases.ToList();
                    }

                    switch (valueItem.ColumnHeader.Indicator.ValueKind)
                    {
                        case IndicatorKind.Cases:
                            valueItem.Value = selectedCases.Count();
                            break;

                        case IndicatorKind.Services:
                            valueItem.Value = selectedCases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
                            break;

                        case IndicatorKind.BedDays:
                            valueItem.Value = selectedCases.Sum(x => x.BedDays);
                            break;

                        case IndicatorKind.LaborCost:
                            valueItem.Value = selectedCases
                                .SelectMany(x => x.Services)
                                .GroupBy(x => x.Code)
                                .Select(x => new { Code = x.Key, Count = x.Sum(y => y.Count) })
                                .Join(classifiers, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.LaborCost)
                                .Sum();
                            break;

                        case IndicatorKind.Cost:
                            valueItem.Value = selectedCases
                                .Where(x => x.PaidStatus == PaidKind.None)
                                .SelectMany(x => x.Services)
                                .GroupBy(x => x.Code)
                                .Select(x => new { Code = x.Key, Count = x.Sum(y => y.Count) })
                                .Join(classifiers, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.Price)
                                .Sum()
                                + selectedCases
                                .Where(x => x.PaidStatus == PaidKind.Full || x.PaidStatus == PaidKind.Partly)
                                .Sum(x => x.AmountPaid);
                            break;
                    }

                    valueItem.Value = valueItem.Value * column.Indicator.MultiplicationFactor / column.Indicator.DivideFactor;
                }
            }

            //заполняет план
            foreach (var row in Rows.Where(x => x.Parameter.Kind == ParameterKind.EmployeePlan || x.Parameter.Kind == ParameterKind.DepartmentHandPlan))
                foreach (var column in Columns.Where(x => x.Group.Component.IsCanPlanning))
                {
                    var valueItem = Values[row.Index, column.Index];

                    var plan = plans.Where(x => x.Parameter.Id == row.Parameter.Id && x.Indicator.Id == column.Indicator.Id).FirstOrDefault();

                    valueItem.ValueContext = plan;
                    valueItem.Value = plan.Value;
                }

            //суммирует строки
            foreach (var row in Rows.Where(x => x.Parameter.Kind == ParameterKind.DepartmentCalculatedPlan || x.Parameter.Kind == ParameterKind.DepartmentFact || x.Parameter.Kind == ParameterKind.DepartmentRejectedFact))
                foreach (var column in Columns.OrderByDescending(x => x.Priority))
                {
                    var valueItem = Values[row.Index, column.Index];
                    valueItem.Value = 0;

                    row.Group.Childs
                        .SelectMany(x => x.HeaderItems)
                        .Where(x => x.Parameter.Kind == row.Parameter.Kind)
                        .ToList()
                        .ForEach(x => valueItem.Value += Values[x.Index, column.Index].Value);
                }

            //суммирует столбцы
            foreach (var row in Rows.OrderByDescending(x => x.Priority))
                foreach (var column in Columns.Where(x => x.Group.Component.CaseFilters.First().Kind == CaseFilterKind.Total))
                {
                    var valueItem = Values[row.Index, column.Index];
                    valueItem.Value = 0;

                    column.Group.Childs
                        .SelectMany(x => x.HeaderItems)
                        .Where(x => x.Indicator.FacadeKind == column.Indicator.ValueKind)
                        .ToList()
                        .ForEach(x => valueItem.Value += Values[row.Index, x.Index].Value);
                }

            //вычисляет проценты в штатных единицах
            foreach (var row in Rows.Where(x => x.Parameter.Kind == ParameterKind.EmployeePercent ))
                foreach (var column in Columns)
                {
                    var dividend = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.EmployeePlan).Index, column.Index].Value;
                    var divider = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.EmployeeFact).Index, column.Index].Value;

                    Values[row.Index, column.Index].Value = dividend / divider;
                }

            //вычисляет проценты в подразделениях
            foreach (var row in Rows.Where(x =>  x.Parameter.Kind == ParameterKind.DepartmentPercent))
                foreach (var column in Columns)
                {
                    var dividend = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.DepartmentHandPlan).Index, column.Index].Value;
                    var divider = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.DepartmentFact).Index, column.Index].Value;

                    Values[row.Index, column.Index].Value = dividend / divider;
                }

        }



    }
}
