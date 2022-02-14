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
        List<IndicatorBase> indicators;
        List<Department> departments;
        List<Employee> employees;
        List<Component> components;


        public int MoneyRoundDigits { get; set; } = 0;
        public int Month { get; private set; }
        public int Year { get; private set; }
        public bool IsGrowing { get; private set; }
        public Dictionary<(Parameter, IndicatorBase), double?> Results { get; set; }


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
                    SetValuesFromCases(register.GetPaidCases(), currentMonth, year, true);

                    //заполняет ошибки (снятия)
                    SetValuesFromCases(register.GetRefusedCases(), currentMonth, year, false);
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

                foreach (var indicator in indicators.Where(x => !x.Component.IsTotal))
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
                foreach (var indicator in indicators.Where(x => x.Component.IsTotal).Reverse())
                {
                    double sum = 0;

                    var indicatorType = indicator.GetType();
                    var indicatorEqTypes = new List<Type> { indicatorType };

                    if (indicatorType == typeof(CasesIndicator))
                        indicatorEqTypes.Add(typeof(CasesLaborCostIndicator));
                    else if (indicatorType == typeof(VisitsIndicator))
                        indicatorEqTypes.Add(typeof(VisitsLaborCostIndicator));

                    indicator.Component.Childs
                        .SelectMany(x => x.Indicators)
                        .Where(x => indicatorEqTypes.Contains(x.GetType()))
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

                else if (key.Item2.GetType() == typeof(CostIndicator))
                    result = Math.Round(result.Value, MoneyRoundDigits, MidpointRounding.AwayFromZero);

                else
                    result = Math.Round(result.Value, 0, MidpointRounding.AwayFromZero);

                Results[key] = result;
            }
        }

        void SetValuesFromCases(IEnumerable<Case> cases, int periodMonth, int periodYear, bool isPaymentAccepted)
        {
            var parameterKind = isPaymentAccepted ? ParameterKind.EmployeeFact : ParameterKind.EmployeeRejectedFact;

            foreach (var employeeCasesGroup in cases.GroupBy(x => x.Employee))
                foreach (Component component in components.Where(x => !x.IsTotal))
                {
                    //возможное место для оптимизации: В методе ApplyFilters на каждой штатной единице отбираются одни и те же фильтры для одного периода
                    var selectedCases = component.ApplyFilters(employeeCasesGroup.ToList(), periodMonth, periodYear).ToList();

                    //расчитывает значения Results
                    foreach (var parameter in employeeCasesGroup.Key.Parameters.Where(x => x.Kind == parameterKind))
                        foreach (var indicator in component.Indicators)
                            Results[(parameter, indicator)] += indicator.CalculateValue(selectedCases, isPaymentAccepted, periodMonth, periodYear);
                }
        }
    }
}
