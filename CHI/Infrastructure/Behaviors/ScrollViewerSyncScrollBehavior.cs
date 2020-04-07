using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace CHI.Infrastructure
{ 
    public class ScrollViewerSyncScrollBehavior : Behavior<FrameworkElement>
    {
        ScrollViewer scrollViewer;

        public static DependencyProperty SyncToProperty { get; set; }
        public ScrollViewer SyncTo { get => (ScrollViewer)GetValue(SyncToProperty); set => SetValue(SyncToProperty, value); }
        public bool SyncVertical { get; set; }
        public bool SyncHorizontal { get; set; }

        static ScrollViewerSyncScrollBehavior()
        {
            SyncToProperty = DependencyProperty.Register(
                                   "SyncTo",
                                   typeof(ScrollViewer),
                                   typeof(ScrollViewerSyncScrollBehavior));
        }

        protected override void OnAttached()
        {
            scrollViewer = (ScrollViewer)AssociatedObject;

            scrollViewer.ScrollChanged += OnScrollChanged;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (SyncHorizontal)
            SyncTo.ScrollToHorizontalOffset(e.HorizontalOffset);
            if (SyncVertical)
                SyncTo.ScrollToVerticalOffset(e.VerticalOffset);

        }

        protected override void OnDetaching()
        {
            scrollViewer.ScrollChanged -= OnScrollChanged;
        }

    }
}
