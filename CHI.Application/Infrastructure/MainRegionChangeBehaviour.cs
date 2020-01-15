using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CHI.Application.Infrastructure
{

    /// <summary>
    /// Анимация смены представления в главном регионе
    /// </summary>
    public class MainRegionChangeBehaviour : Behavior<FrameworkElement>
    {
        private DockPanel panel;
        private Window window;

        protected override void OnAttached()
        {
            panel = AssociatedObject.FindLogicalParent<DockPanel>();
            window = AssociatedObject.FindLogicalParent<Window>();

            AssociatedObject.TargetUpdated += EventHandler;
        }

        private void EventHandler(object sender, DataTransferEventArgs e)
        {
            var elipseGeometry = new EllipseGeometry(Mouse.GetPosition(panel), 0, 0);
            var point = Mouse.GetPosition(window);
            var x = Math.Max(point.X, window.ActualWidth - point.X);
            var y = Math.Max(point.Y, window.ActualHeight - point.Y);
            var radius = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            panel.Clip = elipseGeometry;

            var duration = TimeSpan.FromMilliseconds(500);

            var ease = new ExponentialEase();
            ease.EasingMode = EasingMode.EaseIn;
            ease.Exponent = 1.5;

            var animationOpacity = new DoubleAnimation(0, 1, duration);
            animationOpacity.EasingFunction = ease;
            var animationX = new DoubleAnimation(0, radius, duration);
            animationX.EasingFunction = ease;
            var animationY = new DoubleAnimation(0, radius, duration);
            animationY.Completed += (sndr, args) => { panel.Clip = null; };
            animationY.EasingFunction = ease;


            panel.BeginAnimation(UIElement.OpacityProperty, animationOpacity);
            elipseGeometry.BeginAnimation(EllipseGeometry.RadiusXProperty, animationX);
            elipseGeometry.BeginAnimation(EllipseGeometry.RadiusYProperty, animationY);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TargetUpdated -= EventHandler;
        }
    }
}
