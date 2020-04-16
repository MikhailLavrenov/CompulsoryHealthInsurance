using System;
using System.ComponentModel;

namespace CHI.Infrastructure
{
    [AttributeUsage(AttributeTargets.All)]
    public class MultipleDescriptionAttribute : DescriptionAttribute
    {
        public string ShortDescription { get; set; }


        public MultipleDescriptionAttribute()
            : base()
        {
            ShortDescription = string.Empty;
        }

        public MultipleDescriptionAttribute(string description,string shortDescription)
            : base(description)
        {
            ShortDescription = shortDescription;
        }


    }
}
