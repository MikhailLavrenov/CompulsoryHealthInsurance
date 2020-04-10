using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class ReportService
    {
        RowHeaderGroup rootRow;
        ColumnHeaderGroup rootColumn;

        public List<RowHeaderGroup> RowGroups { get; set; }
        public List<ColumnHeaderGroup> ColumnGroups { get; set; }
        public List<RowHeaderItem> RowItems { get; private set; }
        public List<ColumnHeaderItem> ColumnItems { get; private set; }
        public ValueItem[][] Values { get; private set; }


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

            ColumnGroups = rootColumn.ToListRecursive().Skip(1).ToList();
            ColumnItems = ColumnGroups.SelectMany(x => x.HeaderItems).ToList();

            Enumerable.Range(0, ColumnItems.Count).ToList().ForEach(x => ColumnItems[x].Index = x);
            Enumerable.Range(0, ColumnGroups.Count).ToList().ForEach(x => ColumnGroups[x].Index = x);


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

            RowGroups = rootRow.ToListRecursive().Skip(1).ToList();
            RowItems = RowGroups.SelectMany(x => x.HeaderItems).ToList();

            Enumerable.Range(0, RowItems.Count).ToList().ForEach(x => RowItems[x].Index = x);
            Enumerable.Range(0, RowGroups.Count).ToList().ForEach(x => RowGroups[x].Index = x);

            Values = new ValueItem[RowItems.Count][];

            for (int row = 0; row < RowItems.Count; row++)
            {
                Values[row] = new ValueItem[indicators.Count];

                for (int col = 0; col < ColumnItems.Count; col++)
                    Values[row][col] = new ValueItem(row, col, RowItems[row], ColumnItems[col]);
            }
        }

        public void Build(List<Register> registers, List<Plan> plans, List<ServiceClassifier> classifiers, int monthBegin, int monthEnd, int year)
        {
            foreach (var valueItemsRow in Values)
                foreach (var valueItem in valueItemsRow)
                    valueItem.Value = 0;

            //заполняет план
            foreach (var row in RowItems.Where(x => x.Parameter.Kind == ParameterKind.EmployeePlan || x.Parameter.Kind == ParameterKind.DepartmentHandPlan))
                foreach (var column in ColumnItems.Where(x => x.Group.Component.IsCanPlanning))
                {
                    var valueItem = Values[row.Index][column.Index];

                    valueItem.Value = plans.Where(x => x.Parameter.Id == row.Parameter.Id && x.Indicator.Id == column.Indicator.Id).Sum(x => x.Value);
                }


            for (int month = monthBegin; month <= monthEnd; month++)
            {
                var cases = registers.FirstOrDefault(x => x.Month == month)?.Cases ?? new List<Case>();
                var classifier = classifiers.FirstOrDefault(x => BetweenDates(x.ValidFrom, x.ValidTo, month, year))?.ServiceClassifierItems ?? new List<ServiceClassifierItem>();

                //заполняет факт
                SetValuesFromCases(month, year, cases, classifier, true);

                //заполняет ошибки (снятия)
                SetValuesFromCases(month, year, cases, classifier, false);
            }


            //суммирует строки
            foreach (var row in RowItems
                .Where(x => x.Parameter.Kind == ParameterKind.DepartmentCalculatedPlan || x.Parameter.Kind == ParameterKind.DepartmentFact || x.Parameter.Kind == ParameterKind.DepartmentRejectedFact)
                .OrderByDescending(x => x.Priority))
                foreach (var column in ColumnItems)
                {
                    var valueItem = Values[row.Index][column.Index];

                    row.Group.Childs
                        .SelectMany(x => x.HeaderItems)
                        .Where(x => x.Parameter.Kind == row.Parameter.Kind)
                        .ToList()
                        .ForEach(x => valueItem.Value += Values[x.Index][column.Index].Value ?? 0);
                }

            ////суммирует столбцы
            //foreach (var row in RowItems)
            //    foreach (var column in ColumnItems.Where(x => x.Group.Component.CaseFilters.First().Kind == CaseFilterKind.Total).OrderByDescending(x => x.Priority))
            //    {
            //        var valueItem = Values[row.Index][column.Index];
            //        valueItem.Value = 0;

            //        column.Group.Childs
            //            .SelectMany(x => x.HeaderItems)
            //            .Where(x => x.Indicator.FacadeKind == column.Indicator.ValueKind)
            //            .ToList()
            //            .ForEach(x => valueItem.Value += Values[row.Index][x.Index].Value ?? 0);
            //    }

            ////вычисляет проценты в штатных единицах
            //foreach (var row in RowItems.Where(x => x.Parameter.Kind == ParameterKind.EmployeePercent))
            //    foreach (var column in ColumnItems)
            //    {
            //        var dividend = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.EmployeePlan).Index][column.Index].Value;
            //        var divider = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.EmployeeFact).Index][column.Index].Value;

            //        if (divider != 0)
            //            Values[row.Index][column.Index].Value = dividend / divider;
            //    }

            ////вычисляет проценты в подразделениях
            //foreach (var row in RowItems.Where(x => x.Parameter.Kind == ParameterKind.DepartmentPercent))
            //    foreach (var column in ColumnItems)
            //    {
            //        var dividend = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.DepartmentHandPlan).Index][column.Index].Value;
            //        var divider = Values[row.Group.HeaderItems.First(x => x.Parameter.Kind == ParameterKind.DepartmentFact).Index][column.Index].Value;

            //        if (divider != 0)
            //            Values[row.Index][column.Index].Value = dividend / divider;
            //    }

        }

        //private void SetValuesFromCases(int month, int year, List<Case> cases, List<ServiceClassifierItem> classifierItems, bool isPaymentAccepted)
        //{
        //    cases = cases.Where(x => x.PaidStatus != PaidKind.Refuse == isPaymentAccepted).ToList();

        //    foreach (var row in RowItems.Where(x => x.Parameter.Kind == ParameterKind.EmployeeFact))
        //    {
        //        var employeeCases = cases.Where(x => x.Employee == row.Parameter.Employee).ToList();

        //        ColumnHeaderGroup lastGroup = null;
        //        IEnumerable<Case> selectedCases = null;

        //        foreach (var column in ColumnItems.Where(x => x.Group.Component.CaseFilters.First().Kind != CaseFilterKind.Total))
        //        {
        //            var valueItem = Values[row.Index][column.Index];

        //            if (lastGroup == null || lastGroup != valueItem.ColumnHeader.Group)
        //            {
        //                //IEnumerable<Case> query= factCases;
        //                //selectedCases = new List<Case>();

        //                selectedCases = employeeCases;

        //                if (column.Group.TreatmentCodes.Any())
        //                    selectedCases = selectedCases.Where(x => column.Group.TreatmentCodes.Contains(x.TreatmentPurpose));
        //                if (column.Group.VisitFilters.Any())
        //                    selectedCases = selectedCases.Where(x => column.Group.VisitFilters.Contains(x.VisitPurpose));
        //                if (column.Group.ContainsServiceFilters.Any())
        //                    selectedCases = selectedCases.Where(x => x.Services.Any(y => column.Group.ContainsServiceFilters.Contains(y.Code)));
        //                if (column.Group.NotContainsServiceFilters.Any())
        //                    selectedCases = selectedCases.Where(x => x.Services.Any(y => !column.Group.NotContainsServiceFilters.Contains(y.Code)));

        //                selectedCases = selectedCases.ToList();
        //            }

        //            switch (valueItem.ColumnHeader.Indicator.ValueKind)
        //            {
        //                case IndicatorKind.Cases:
        //                    valueItem.Value = selectedCases.Count();
        //                    break;

        //                case IndicatorKind.Services:
        //                    valueItem.Value = selectedCases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
        //                    break;

        //                case IndicatorKind.BedDays:
        //                    valueItem.Value = selectedCases.Sum(x => x.BedDays);
        //                    break;

        //                case IndicatorKind.LaborCost:
        //                    valueItem.Value = selectedCases
        //                        .SelectMany(x => x.Services)
        //                        .GroupBy(x => x.Code)
        //                        .Select(x => new { Code = x.Key, Count = x.Sum(y => y.Count) })
        //                        .Join(classifierItems, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.LaborCost)
        //                        .Sum();
        //                    break;

        //                case IndicatorKind.Cost:
        //                    if (isPaymentAccepted)
        //                        valueItem.Value = selectedCases
        //                            .Where(x => x.PaidStatus == PaidKind.None)
        //                            .SelectMany(x => x.Services)
        //                            .GroupBy(x => x.Code)
        //                            .Select(x => new { Code = x.Key, Count = x.Sum(y => y.Count) })
        //                            .Join(classifierItems, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.Price)
        //                            .Sum()
        //                            + selectedCases
        //                            .Where(x => x.PaidStatus == PaidKind.Full || x.PaidStatus == PaidKind.Partly)
        //                            .Sum(x => x.AmountPaid);
        //                    else
        //                        valueItem.Value = selectedCases
        //                            .Where(x => x.PaidStatus == PaidKind.Refuse || x.PaidStatus == PaidKind.Partly)
        //                            .Sum(x => x.AmountUnpaid);
        //                    break;
        //            }

        //            var ratio = column.Indicator.Ratios.First();

        //            valueItem.Value = valueItem.Value * ratio.Multiplier / ratio.Divider;
        //        }
        //    }

        //}

        private void SetValuesFromCases(int month, int year, IEnumerable<Case> cases, List<ServiceClassifierItem> classifierItems, bool isPaymentAccepted)
        {
            var filters = ColumnGroups.Select(x => new
            {
                TreatmentCodes = x.TreatmentFilters.Where(y => BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                VisitCodes = x.VisitFilters.Where(y => BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                ContainsServiceCodes = x.ContainsServiceFilters.Where(y => BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList(),
                NotContainsServiceCodes = x.NotContainsServiceFilters.Where(y => BetweenDates(y.ValidFrom, y.ValidTo, month, year)).Select(x => x.Code).ToList()
            })
                .ToList();

            cases = cases.Where(x => x.PaidStatus != PaidKind.Refuse == isPaymentAccepted).ToList();

            foreach (var rowGroup in RowGroups.Where(x => x.Employee != null))
            {
                var employeeCases = cases.Where(x => x.Employee.Id == rowGroup.Employee.Id).ToList();

                foreach (var columnGroup in ColumnGroups.Where(x => x.Component.CaseFilters.First().Kind != CaseFilterKind.Total))
                {
                    IEnumerable<Case> selectedCases = employeeCases;

                    var filter = filters[columnGroup.Index];

                    if (filter.TreatmentCodes.Any())
                        selectedCases = selectedCases.Where(x => filter.TreatmentCodes.Contains(x.TreatmentPurpose));
                    if (filter.VisitCodes.Any())
                        selectedCases = selectedCases.Where(x => filter.VisitCodes.Contains(x.VisitPurpose));
                    if (filter.ContainsServiceCodes.Any())
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => filter.ContainsServiceCodes.Contains(y.Code)));
                    if (filter.NotContainsServiceCodes.Any())
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => !filter.NotContainsServiceCodes.Contains(y.Code)));

                    selectedCases = selectedCases.ToList();

                    foreach (var rowItem in rowGroup.HeaderItems.Where(x=> (isPaymentAccepted && x.Parameter.Kind== ParameterKind.EmployeeFact) || (!isPaymentAccepted && x.Parameter.Kind == ParameterKind.EmployeeRejectedFact)))
                        foreach (var columnItem in columnGroup.HeaderItems)
                        {
                            var valueItem = Values[rowItem.Index][columnItem.Index];

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
                                        .Join(classifierItems, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.LaborCost)
                                        .Sum();
                                    break;

                                case IndicatorKind.Cost:
                                    if (isPaymentAccepted)
                                        valueItem.Value = selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.None)
                                            .SelectMany(x => x.Services)
                                            .GroupBy(x => x.Code)
                                            .Select(x => new { Code = x.Key, Count = x.Sum(y => y.Count) })
                                            .Join(classifierItems, service => service.Code, classifier => classifier.Code, (service, classifier) => service.Count * classifier.Price)
                                            .Sum()
                                            + selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.Full || x.PaidStatus == PaidKind.Partly)
                                            .Sum(x => x.AmountPaid);
                                    else
                                        valueItem.Value = selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.Refuse || x.PaidStatus == PaidKind.Partly)
                                            .Sum(x => x.AmountUnpaid);
                                    break;
                            }

                            var ratio = columnItem.Indicator.Ratios.FirstOrDefault(x => BetweenDates(x.ValidFrom, x.ValidTo, month, year));

                            if (ratio != null)
                                valueItem.Value = valueItem.Value * ratio.Multiplier / ratio.Divider;
                        }
                }
            }

        }


        private static bool BetweenDates(DateTime? date1, DateTime? date2, int periodMonth, int periodYear)
        {
            var limit1 = date1.HasValue ? date1.Value.Year * 100 + date1.Value.Month : 0;
            var limit2 = date2.HasValue ? date2.Value.Year * 100 + date2.Value.Month : int.MaxValue;
            var period = periodYear * 100 + periodMonth;

            return limit1 <= period && period <= limit2;
        }

    }
}
