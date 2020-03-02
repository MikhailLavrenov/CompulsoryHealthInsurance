using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace CHI.Infrastructure
{

    /// <summary>
    ///     Позволяет перемещать окно мышью.
    /// </summary>
    public class DragWindowBehavior : Behavior<FrameworkElement>
    {
        private Window window;

        protected override void OnAttached()
        {
            window = AssociatedObject as Window;

            if (window==null)
                window = Window.GetWindow(AssociatedObject);
            
            if (window == null) 
                return;

            AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDownHandler;
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            window.DragMove();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDownHandler;
            window = null;
        }
    }
}
