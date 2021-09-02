using CHI.Infrastructure;
using CHI.Models.ServiceAccounting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.Report
{
    public class ReportService
    {
        List<Parameter> parameters;
        List<Indicator> indicators;
        List<Department> departments;
        List<Employee> employees;
        List<Component> components;


        public int MoneyRoundDigits { get; set; } = 0;
        public int Month { get; private set; }
        public int Year { get; private set; }
        public bool IsGrowing { get; private set; }
        public Dictionary<(Parameter, Indicator), double?> Results { get; set; }


        public ReportService(Department rootDepartment, Component rootComponent)
        {
            departments = rootDepartment.ToListRecursive();
            employees = departments.SelectMany(x => x.Employees).ToList();
            components = rootComponent.ToListRecursive();
            parameters = departments.SelectMany(x => x.Parameters).ToList().Concat(employees.SelectMany(x => x.Parameters).ToList()).ToList();
            indicators = components.SelectMany(x => x.Indicators).ToList();
            Results = parameters.SelectMany(x => indicators, (x, y) => (x, y)).ToDictionary(x => x, x => (double?)null);
        }


        public void Build(List<Register> registers, List<Plan> plans, int month, int year, bool isGrowing = false)
        {
            Month = month;
            Year = year;
            IsGrowing = isGrowing;

            foreach (var key in Results.Keys.ToList())
                Results[key] = 0;

            SetPlans(plans);

            if (registers?.Any() ?? false)
                for (var currentMonth = isGrowing ? 1 : month; currentMonth <= month; currentMonth++)
                {
                    var register = registers.FirstOrDefault(x => x.Month == currentMonth);

                    if (register == null)
                        continue;

                    //заполняет факт
                    SetValuesFromCases(currentMonth, year, register.GetPaidCases(), true);

                    //заполняет ошибки (снятия)
                    SetValuesFromCases(currentMonth, year, register.GetRefusedCases(), false);
                }

            SumRows();

            SumColumns();

            SetPercentsInEmployees();

            SetPercentsInDepartments();

            DropZeroAndRoundValues();
        }

        public void UpdateCalculatedCells()
        {
            SumRows();

            SumColumns();

            DropZeroAndRoundValues();
        }

        void SetPlans(List<Plan> plans)
        {
            if (!(plans?.Any() ?? false))
                return;

            foreach (var parameter in parameters.Where(x => x.Kind == ParameterKind.EmployeePlan || x.Kind == ParameterKind.DepartmentHandPlan))
                foreach (var indicator in indicators.Where(x => x.Component.IsCanPlanning))
                    Results[(parameter, indicator)] = plans.Where(x => x.Parameter.Id == parameter.Id && x.Indicator.Id == indicator.Id).Sum(x => x.Value);
        }

        void SumRows()
        {
            foreach (var parameter in parameters.Where(x => x.Department != null).Reverse())
            {
                ParameterKind eqEmployeeKind = parameter.Kind switch
                {
                    ParameterKind.DepartmentCalculatedPlan => ParameterKind.EmployeePlan,
                    ParameterKind.DepartmentHandPlan when parameter.Department.Childs.Any() => ParameterKind.DepartmentHandPlan,
                    ParameterKind.DepartmentFact => ParameterKind.EmployeeFact,
                    ParameterKind.DepartmentRejectedFact => ParameterKind.EmployeeRejectedFact,
                    _ => ParameterKind.None,
                };

                if (eqEmployeeKind == ParameterKind.None)
                    continue;

                foreach (var indicator in indicators.Where(x => x.Component.CaseFilters.First().Kind != CaseFilterKind.Total))
                {
                    double sum = 0;

                    parameter.Department.Childs
                        .SelectMany(x => x.Parameters).Concat(parameter.Department.Employees.SelectMany(y => y.Parameters))
                        .Where(x => x.Kind == parameter.Kind || x.Kind == eqEmployeeKind)
                        .ToList()
                        .ForEach(x => sum += Results[(x, indicator)] ?? 0);

                    Results[(parameter, indicator)] = sum;
                }
            }
        }

        void SumColumns()
        {
            foreach (var parameter in parameters)
                foreach (var indicator in indicators.Where(x => x.Component.CaseFilters.First().Kind == CaseFilterKind.Total).Reverse())
                {
                    double sum = 0;

                    indicator.Component.Childs
                        .SelectMany(x => x.Indicators)
                        .Where(x => x.FacadeKind == indicator.ValueKind)
                        .ToList()
                        .ForEach(x => sum += Results[(parameter, x)] ?? 0);

                    Results[(parameter, indicator)] = sum;
                }
        }

        void SetPercentsInEmployees()
        {
            foreach (var employee in employees.Where(x => x.Parameters.Any()))
            {

                var dividendParameter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeeFact).FirstOrDefault();
                var dividerParameter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeePlan).FirstOrDefault();
                var percentParamenter = employee.Parameters.Where(x => x.Kind == ParameterKind.EmployeePercent).FirstOrDefault();

                if (percentParamenter == null || dividendParameter == null || dividerParameter == null)
                    continue;

                foreach (var indicator in indicators)
                {
                    var dividend = Results[(dividendParameter, indicator)];
                    var divider = Results[(dividerParameter, indicator)];
                    Results[(percentParamenter, indicator)] = divider == 0 ? 0 : dividend / divider * 100;
                }
            }
        }

        void SetPercentsInDepartments()
        {
            foreach (var department in departments.Where(x => x.Parameters.Any()))
            {
                var dividendParameter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentFact).FirstOrDefault();
                var dividerParameter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentHandPlan).FirstOrDefault();
                var percentParamenter = department.Parameters.Where(x => x.Kind == ParameterKind.DepartmentPercent).FirstOrDefault();

                if (percentParamenter == null || dividendParameter == null || dividerParameter == null)
                    continue;

                foreach (var indicator in indicators)
                {
                    var dividend = Results[(dividendParameter, indicator)];
                    var divider = Results[(dividerParameter, indicator)];
                    Results[(percentParamenter, indicator)] = divider == 0 ? 0 : dividend / divider * 100;
                }
            }
        }

        void DropZeroAndRoundValues()
        {
            foreach (var key in Results.Keys.ToList())
            {
                var result = Results[key];

                if (result == null)
                    continue;

                if (result == 0)
                    result = null;
                else if (key.Item1.Kind == ParameterKind.DepartmentPercent)
                    result = Math.Round(result.Value, 1, MidpointRounding.AwayFromZero);

                else if (key.Item2.FacadeKind == IndicatorKind.Cost)
                    result = Math.Round(result.Value, MoneyRoundDigits, MidpointRounding.AwayFromZero);

                else
                    result = Math.Round(result.Value, 0, MidpointRounding.AwayFromZero);

                Results[key] = result;
            }
        }

        void SetValuesFromCases(int periodMonth, int periodYear, IEnumerable<Case> cases, bool isPaymentAccepted)
        {
            //оптимизация чтобы не выполнять одинаковые действия в разных итерациях
            var employeesCases = cases.GroupBy(x => x.Employee)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var component in components.Where(x => x.CaseFilters.Any() && x.CaseFilters.First().Kind != CaseFilterKind.Total))
            {
                //отбирает фильтры которые удовлетворяют заданому месяцу и году
                var groupedFilterCodes = component.CaseFilters
                    .Where(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, periodMonth, periodYear))
                    .GroupBy(x => x.Kind)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.Code).ToList());

                foreach (var employeeCases in employeesCases)
                {
                    //отбирает случаи которые удовлетворяют фильтрам
                    IEnumerable<Case> selectedCases = employeeCases.Value;

                    if (groupedFilterCodes.ContainsKey(CaseFilterKind.TreatmentPurpose))
                        selectedCases = selectedCases.Where(x => groupedFilterCodes[CaseFilterKind.TreatmentPurpose].Contains(x.TreatmentPurpose));
                    if (groupedFilterCodes.ContainsKey(CaseFilterKind.VisitPurpose))
                        selectedCases = selectedCases.Where(x => groupedFilterCodes[CaseFilterKind.VisitPurpose].Contains(x.VisitPurpose));
                    if (groupedFilterCodes.ContainsKey(CaseFilterKind.ContainsService))
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => groupedFilterCodes[CaseFilterKind.ContainsService].Contains(y.Code)));
                    if (groupedFilterCodes.ContainsKey(CaseFilterKind.NotContainsService))
                        selectedCases = selectedCases.Where(x => x.Services.Any(y => !groupedFilterCodes[CaseFilterKind.NotContainsService].Contains(y.Code)));

                    selectedCases = selectedCases.ToList();

                    //расчитывает значения Results
                    foreach (var parameter in employeeCases.Key.Parameters.Where(x => (isPaymentAccepted && x.Kind == ParameterKind.EmployeeFact) || (!isPaymentAccepted && x.Kind == ParameterKind.EmployeeRejectedFact)))
                        foreach (var indicator in component.Indicators)
                        {
                            double value = 0;

                            switch (indicator.ValueKind)
                            {
                                case IndicatorKind.Cases:
                                    value = selectedCases.Count();
                                    break;

                                case IndicatorKind.Services:
                                    value = selectedCases.Select(x => x.Services.Count == 0 ? 0 : x.Services.Count - 1).Sum();
                                    break;

                                case IndicatorKind.BedDays:
                                    value = selectedCases.Sum(x => x.BedDays);
                                    break;

                                case IndicatorKind.LaborCost:
                                    value = selectedCases
                                        .SelectMany(x => x.Services)
                                        .Where(x => x.ClassifierItem != null)
                                        .Sum(x => x.Count * x.ClassifierItem.LaborCost);
                                    break;

                                case IndicatorKind.Cost:
                                    if (isPaymentAccepted)
                                        value = selectedCases
                                            .Where(x => x.PaidStatus == PaidKind.None)
                                            .SelectMany(x => x.Services)
                                            .Where(x => x.ClassifierItem != null)
                                            .Sum(x => x.Count * x.ClassifierItem.Price)
                                            + selectedCases
                                            .Where(x => x.PaidStatus != PaidKind.None)
                                            .Sum(x => x.AmountPaid);
                                    else
                                        value = selectedCases.Sum(x => x.AmountUnpaid);
                                    break;
                            }

                            var ratio = indicator.Ratios.FirstOrDefault(x => Helpers.BetweenDates(x.ValidFrom, x.ValidTo, periodMonth, periodYear));

                            Results[(parameter, indicator)] += ratio is null ? value : value * ratio.Multiplier / ratio.Divider;
                        }
                }
            }

        }
    }
}
