using Prism.Mvvm;
using System.Linq;

namespace CHI.Infrastructure
{
    public class HeaderSubItem : BindableBase
    {
        bool isSelected = false;

        public string Name { get; }
        public HeaderItem HeaderItem { get; }
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
        public bool IsFirstAndGroupCanCollapse { get; }

        public HeaderSubItem(string name, HeaderItem headerItem,bool isFirstInGroup)
        {
            Name = name;
            HeaderItem = headerItem;
            IsFirstAndGroupCanCollapse = HeaderItem.CanCollapse == true && isFirstInGroup;
        }
    }
}
