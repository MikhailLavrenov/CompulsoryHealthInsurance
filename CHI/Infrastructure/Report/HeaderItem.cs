using CHI.Models.ServiceAccounting;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public class HeaderItem : BindableBase, IHierarchical<HeaderItem>
    {
        bool? isCollapsed;
        bool isVisible = true;
        bool alwaysHidden = false;
        Color color;

        public string Name { get; }
        public string SubName { get; }
        public bool IsColorAlternation { get; set; }
        public Color Color { get => color; set => SetProperty(ref color, value); }
        public bool CanCollapse { get; private set; }
        public bool? IsCollapsed { get => isCollapsed; private set => SetProperty(ref isCollapsed, value); }
        public bool AlwaysHidden
        {
            get => alwaysHidden;
            set
            {
                if (alwaysHidden == value)
                    return;

                alwaysHidden = value;

                UpdateVisibility();
            }
        }
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value)
                    return;

                SetProperty(ref isVisible, value);

                UpdateChildrenVisibility();
            }
        }
        public int Level { get; }
        public HeaderItem Parent { get; set; }
        public List<HeaderItem> Childs { get; set; }
        public List<HeaderSubItem> SubItems { get; }

        public DelegateCommand SwitchCollapseCommand { get; }

        public HeaderItem(string name, string subName, string hexColor, bool isColorAlternation, bool alwaysHidden, bool haveChilds, HeaderItem parent, List<string> subItemNames)
        {
            Name = name;
            SubName = subName;
            IsColorAlternation = isColorAlternation;
            Color = string.IsNullOrEmpty(hexColor) ? Colors.Transparent :(Color)ColorConverter.ConvertFromString(hexColor) ;
            CanCollapse = haveChilds;
            IsCollapsed = CanCollapse ? false : (bool?)null;
            AlwaysHidden = alwaysHidden;
            Parent = parent;
            Parent?.Childs.Add(this);
            Level = Parent == null ? 0 : parent.Level + 1;
            SubItems = subItemNames?.Select(x => new HeaderSubItem(x, this)).ToList() ?? new List<HeaderSubItem>();
            Childs = new List<HeaderItem>();

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }

        private void UpdateVisibility()
        {
            if (alwaysHidden || Parent?.IsVisible==false)
                IsVisible = false;
            else
                IsVisible = !(Parent?.IsCollapsed.Value??false);
        }

        private void UpdateChildrenVisibility()
        {
            foreach (var child in Childs)
                child.UpdateVisibility();
        }

        private void SwitchCollapseExecute()
        {
            if (!CanCollapse)
                return;

            IsCollapsed = !IsCollapsed;

            UpdateChildrenVisibility();
        }



    }
}
