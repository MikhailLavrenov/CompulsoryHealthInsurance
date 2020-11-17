using Prism.Commands;
using Prism.Mvvm;
using System;
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

        public static Color AlternationColor1 { get; } = Colors.WhiteSmoke;
        public static Color AlternationColor2 { get; } = Colors.Transparent;

        public string Name { get; private set; }
        public string SubName { get; private set; }
        public Color Color { get => color; private set => SetProperty(ref color, value); }
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
        public HeaderItem Parent { get; set; }
        public List<HeaderItem> Childs { get; set; }
        public List<HeaderSubItem> SubItems { get; private set; }

        public DelegateCommand SwitchCollapseCommand { get; }

        public HeaderItem(string name, string subName, string hexColor, bool alwaysHidden,bool haveChilds, HeaderItem parent, List<string> subItemNames)
        {
            Name = name;
            SubName = subName;
            Color = string.IsNullOrEmpty(hexColor) ? Colors.Transparent : (Color)ColorConverter.ConvertFromString(hexColor);
            CanCollapse = haveChilds;
            IsCollapsed = CanCollapse ? false : (bool?)null;
            AlwaysHidden = alwaysHidden;
            Parent = parent;
            Parent?.Childs.Add(this);
            SubItems = subItemNames?.Select(x => new HeaderSubItem(x, this)).ToList() ?? new List<HeaderSubItem>();
            Childs = new List<HeaderItem>();

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }

        private void UpdateVisibility()
        {
            if (alwaysHidden || !Parent.IsVisible)
                IsVisible = false;
            else 
                IsVisible = !Parent.IsCollapsed.Value;
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
