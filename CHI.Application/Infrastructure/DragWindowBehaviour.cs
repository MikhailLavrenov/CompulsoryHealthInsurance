using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace CHI.Application.Infrastructure
{

    /// <summary>
    ///     Позволяет перемещать окно мышью.
    /// </summary>
    public class DragWindowBehaviour : Behavior<FrameworkElement>
    {
        private Window window;

        protected override void OnAttached()
        {
            window = AssociatedObject as Window;

            if (window==null)
                window = Window.GetWindow(AssociatedObject);
            
            if (window == null) 
                return;

            AssociatedObject.PreviewMouseLeftButtonDown += _associatedObject_MouseLeftButtonDown;
        }

        private void _associatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window.DragMove();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= _associatedObject_MouseLeftButtonDown;
            window = null;
        }
    }
}
