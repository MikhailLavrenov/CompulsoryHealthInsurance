using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CHI.Infrastructure
{

    /// <summary>
    /// Анимация progressBar
    /// </summary>
    public class ProgressBarBehavior : Behavior<FrameworkElement>
    {
        private Canvas panel;

        protected override void OnAttached()
        {
            panel = AssociatedObject.FindLogicalParent<Canvas>();

            AssociatedObject.Loaded += EventHandler;
            panel.SizeChanged += EventHandler;
        }

        private void EventHandler(object sender, RoutedEventArgs e)
        {
            var diameter = 10;
            var length = panel.ActualWidth + diameter;
            var circlesCount = 6;
            var color = (Color)ColorConverter.ConvertFromString("#FF54ACE0");
            var duration = TimeSpan.FromSeconds(5);

            panel.Children.Clear();

            for (int i = 0; i < circlesCount; i++)
            {
                var transform = new TranslateTransform(-diameter, 0);

                var circle = new Ellipse()
                {
                    Width = diameter,
                    Height = diameter,
                    Fill = new SolidColorBrush(color),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = transform,
                };

                panel.Children.Add(circle);

                var keyFrame = new SplineDoubleKeyFrame(length, KeyTime.FromPercent(0.5), new KeySpline(0, 0.95, 1, 0.05));
                var keyFrames = new DoubleKeyFrameCollection();
                keyFrames.Add(keyFrame);

                var animation = new DoubleAnimationUsingKeyFrames()
                {
                    BeginTime = TimeSpan.FromMilliseconds(130 * i),
                    Duration = duration,
                    KeyFrames = keyFrames,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                transform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
        }

        protected override void OnDetaching()
        {
            panel.Loaded -= EventHandler;
            panel.SizeChanged -= EventHandler;
        }
    }
}
