using System.Windows;
using System.Windows.Controls;

namespace CHI.Infrastructure
{
    public class CustomContentControl : ContentControl
    {
        static CustomContentControl()
        {
            ContentProperty.OverrideMetadata(
                typeof(CustomContentControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentChanged))
                );
        }

        private static void OnContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = dependencyObject as CustomContentControl;

            if (control.ContentChanged != null)
            {
                var args = new DependencyPropertyChangedEventArgs(ContentProperty, e.OldValue, e.NewValue);

                control.ContentChanged(control, args);
            }
        }

        public event DependencyPropertyChangedEventHandler ContentChanged;
    }
}
