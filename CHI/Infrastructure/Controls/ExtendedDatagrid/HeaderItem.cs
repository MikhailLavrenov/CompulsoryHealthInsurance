using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CHI.Infrastructure
{
    public class HeaderItem : BindableBase
    {
        bool? isCollapsed;
        bool isVisible = true;
        bool alwaysHidden = false;
        Color color;

        public static Color AlternationColor1 { get; } = Colors.WhiteSmoke;
        public static Color AlternationColor2 { get; } = Colors.Transparent;

        public string Name { get; private set; }
        public string SubName { get; private set; }
        public Color Color { get => color; private set => SetProperty(ref color, value, () => ColorChangedEvent?.Invoke(this, new EventArgs())); }
        public bool CanCollapse { get => Childs?.Any() ?? false; }
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

                IsVisibleChangedEvent?.Invoke(this, new EventArgs());

                UpdateChildrenVisibility();
            }
        }
        public HeaderItem Parent { get; private set; }
        public List<HeaderItem> Childs { get; private set; }
        public List<HeaderSubItem> SubItems { get; private set; }

        public DelegateCommand SwitchCollapseCommand { get; }

        public event EventHandler IsVisibleChangedEvent;
        public event EventHandler ColorChangedEvent;


        public HeaderItem(string name, string subName, Color color, bool alwaysHidden, HeaderItem parent, List<string> subItemNames)
        {
            Name = name;
            SubName = subName;
            Color = color;
            AlwaysHidden = alwaysHidden;
            Parent = parent;
            Parent?.Childs.Add(this);
            SubItems = subItemNames?.Select(x=>new HeaderSubItem(x, this)).ToList() ?? new List<HeaderSubItem>();
            Childs = new List<HeaderItem>();

            SwitchCollapseCommand = new DelegateCommand(SwitchCollapseExecute, () => CanCollapse);
        }

        private void UpdateVisibility()
        {
            if (alwaysHidden || !Parent.IsVisible)
                IsVisible = false;
            else if (Parent.IsCollapsed == false)
                IsVisible = true;
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
