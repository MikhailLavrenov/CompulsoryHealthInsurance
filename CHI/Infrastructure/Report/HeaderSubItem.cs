using System;
using System.Collections.Generic;
using System.Text;

namespace CHI.Infrastructure
{
    public class HeaderSubItem
    {
        public string Name { get; set; }
        public HeaderItem HeaderItem { get; set; }

        public HeaderSubItem(string name, HeaderItem headerItem)
        {
            Name = name;
            HeaderItem = headerItem;
        }
    }
}
