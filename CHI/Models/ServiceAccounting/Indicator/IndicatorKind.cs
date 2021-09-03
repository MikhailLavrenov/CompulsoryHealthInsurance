using CHI.Infrastructure;
using System.ComponentModel;

namespace CHI.Models.ServiceAccounting
{
    public enum IndicatorKind
    {
        [MultipleDescription("Выберите значение", "Не задано")] None = 0,
        [MultipleDescription("Cлучаи","Случ")] Cases = 1,
        [MultipleDescription("Посещения", "Посещ")] Services = 2,
        [MultipleDescription("УЕТ", "УЕТ")] LaborCost = 3,
        [MultipleDescription("Койко-дни", "КДн")] BedDays = 4,
        [MultipleDescription("Стоимость", "Стоим")] Cost = 5,
    }
}
