using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace CHI.Infrastructure
{
    public class ScrollViewerSyncScrollBehavior : Behavior<FrameworkElement>
    {
        ScrollViewer scrollViewer;

        public static DependencyProperty SyncWithProperty { get; set; }
        public ScrollViewer SyncWith { get => (ScrollViewer)GetValue(SyncWithProperty); set => SetValue(SyncWithProperty, value); }
        public bool SyncVertical { get; set; }
        public bool SyncHorizontal { get; set; }

        static ScrollViewerSyncScrollBehavior()
        {
            SyncWithProperty = DependencyProperty.Register(
                                   nameof(SyncWith),
                                   typeof(ScrollViewer),
                                   typeof(ScrollViewerSyncScrollBehavior));
        }

        protected override void OnAttached()
        {
            scrollViewer = (ScrollViewer)AssociatedObject;

            scrollViewer.ScrollChanged += OnScrollChanged;

            SyncWith.ScrollChanged+= OnScrollChanged;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sw = (ScrollViewer)sender == scrollViewer ? SyncWith : scrollViewer;

            if (SyncHorizontal)
                sw.ScrollToHorizontalOffset(e.HorizontalOffset);
            if (SyncVertical)
                sw.ScrollToVerticalOffset(e.VerticalOffset);
        }

        protected override void OnDetaching()
        {
            scrollViewer.ScrollChanged -= OnScrollChanged;
            SyncWith.ScrollChanged -= OnScrollChanged;
        }

    }
}
