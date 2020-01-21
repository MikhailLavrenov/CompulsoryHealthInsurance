using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CHI.Application.Infrastructure
{

    /// <summary>
    /// Анимация смены представлений TabRadioButton
    /// </summary>
    public class TabRadioButtonBehaviour : Behavior<FrameworkElement>
    {
        private Canvas panel;

        protected override void OnAttached()
        {
            panel = AssociatedObject.FindLogicalParent<Canvas>();

            AssociatedObject.Loaded += EventHandler;
        }

        private void EventHandler(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnDetaching()
        {
            panel.Loaded -= EventHandler;
        }
    }
}
