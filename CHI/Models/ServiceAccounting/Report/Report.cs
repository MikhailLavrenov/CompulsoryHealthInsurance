using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Models.ServiceAccounting
{
    public class Report
    {
        public HeaderItem[] RowHeader { get; set; }
        public HeaderItem[] ColumnHeader { get; set; }
        public ValueItem[,] Values { get; set; }
    }
}
