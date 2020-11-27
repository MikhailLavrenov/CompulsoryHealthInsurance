using Prism.Mvvm;

namespace CHI.Infrastructure
{
    public class HeaderSubItem : BindableBase
    {
        bool isSelected = false;

        public string Name { get; set; }
        public HeaderItem HeaderItem { get; set; }
        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

        public HeaderSubItem(string name, HeaderItem headerItem)
        {
            Name = name;
            HeaderItem = headerItem;
        }
    }
}
