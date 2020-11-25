using CHI.Models.ServiceAccounting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public static class ReportHelper
    {
        static Color alternationColor1 = Colors.White;
        static Color alternationColor2 = Colors.WhiteSmoke;

        public static HeaderItem CreateHeaderItemRecursive(Department department, HeaderItem parent)
        {
            var subItemNames = department.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(department.Name, null, department.HexColor, false, false, true, parent, subItemNames);

            foreach (var child in department.Childs)
                CreateHeaderItemRecursive(child, headerItem);

            foreach (var employee in department.Employees)
            {
                subItemNames = employee.Parameters.Select(x => x.Kind.GetShortDescription()).ToList();

                new HeaderItem(employee.Medic.FullName, employee.Specialty.Name, string.Empty, true, false, false, headerItem, subItemNames);
            }

            return headerItem;
        }

        public static HeaderItem CreateHeaderItemRecursive(Component component, HeaderItem parent)
        {
            var subItemNames = component.Indicators.Select(x => x.FacadeKind.GetShortDescription()).ToList();

            var headerItem = new HeaderItem(component.Name, null, component.HexColor, false, false, component.Childs.Any(), parent, subItemNames);

            if (component.Childs != null)
                foreach (var child in component.Childs)
                    CreateHeaderItemRecursive(child, headerItem);

            return headerItem;
        }

        public static void SetAlternationColor(List<HeaderItem> headerItems)
        {
            var colorSwitch = true;
            HeaderItem previousItemParent = null;

            foreach (var item in headerItems.Where(x => x.IsColorAlternation && !x.AlwaysHidden))
            {
                if (item.Parent != previousItemParent)
                    colorSwitch = true;

                item.Color = colorSwitch ? alternationColor1 : alternationColor2;

                colorSwitch = !colorSwitch;
                previousItemParent = item.Parent;
            }
        }
    }
}
