using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CHI.Infrastructure
{

    /// <summary>
    ///     Позволяет перемещать окно мышью.
    /// </summary>
    public class DragWindowBehavior : Behavior<FrameworkElement>
    {
        Window window;

        protected override void OnAttached()
        {
            window = AssociatedObject as Window;

            if (window == null)
                window = Window.GetWindow(AssociatedObject);

            if (window == null)
                return;

            window.MouseLeftButtonDown += MouseLeftButtonDownHandler;
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is DataGrid == false)
                window.DragMove();
        }

        protected override void OnDetaching()
        {
            window.MouseLeftButtonDown -= MouseLeftButtonDownHandler;
            window = null;
        }
    }
}
