using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PatientsFomsRepository.Infrastructure
{
    public static class EnumHelper
    {
        /// <summary>
        /// Возвращает описание (Description) перечисления (enum)
        /// </summary>
        /// <param name="value">Перечисление (enum)</param>
        /// <returns>Описание</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            Attribute attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false);
            DescriptionAttribute descriptionAttribute = attribute as DescriptionAttribute;

            return descriptionAttribute.Description;
        }
        /// <summary>
        /// Возвращает список всех значений enum и их описаний 
        /// </summary>
        /// <typeparam name="T">Перечисление (enum)</typeparam>
        /// <returns>Перечисление значений и описаний</returns>
        public static IEnumerable< KeyValuePair<Enum, string>> GetAllValuesAndDescriptions(Type T)
        {
            return Enum.GetValues(T)
                .Cast<Enum>()
                .Select(x => new KeyValuePair<Enum, string>(x, x.GetDescription()))
                .ToList();
        }
    }
}
