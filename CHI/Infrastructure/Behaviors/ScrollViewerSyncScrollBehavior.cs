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

            SyncWith.ScrollChanged += OnScrollChanged;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //Свойства VerticalChange и HorizontalChange ScrollChangedEventArgs не всегда верно показывают значение сдвига,
            //поэтому используются только текущие значения ScrollViewer

            var scrolledSV = (ScrollViewer)sender;
            var associatedSV = scrolledSV == scrollViewer ? SyncWith : scrollViewer;

            var isHorizontalChanged = SyncHorizontal ? scrolledSV.HorizontalOffset != associatedSV.HorizontalOffset : false;
            var isVerticalChanged = SyncVertical ? scrolledSV.VerticalOffset != associatedSV.VerticalOffset : false;

            if (!isHorizontalChanged && !isVerticalChanged)
                return;

            associatedSV.ScrollChanged -= OnScrollChanged;

            if (isHorizontalChanged)
                associatedSV.ScrollToHorizontalOffset(scrolledSV.HorizontalOffset);

            if (isVerticalChanged)
                associatedSV.ScrollToVerticalOffset(scrolledSV.VerticalOffset);

            associatedSV.ScrollChanged += OnScrollChanged;
        }

        protected override void OnDetaching()
        {
            scrollViewer.ScrollChanged -= OnScrollChanged;
            SyncWith.ScrollChanged -= OnScrollChanged;
        }

    }
}
