using System.ComponentModel.DataAnnotations.Schema;

namespace CHI.Services.Report
{
    public class ValueItem
    {
        public RowHeaderItem RowHeader { get; set; }
        public ColumnHeaderItem ColumnHeader { get; set; }
        public double Value { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public object ValueContext { get; set; }

        public ValueItem(int rowIndex, int columnIndex, RowHeaderItem rowHeader, ColumnHeaderItem columnHeader)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            RowHeader = rowHeader;
            ColumnHeader = columnHeader;          
        }
    }
}
