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
    /// Базовый класс поведения с круговой анимацией. Используется при смене view в RegionManager
    /// </summary>
    public abstract class CircleAnimationBaseBehaviour : Behavior<FrameworkElement>
    {
        protected CustomContentControl customContentControl;
        protected FrameworkElement animatedElement;
        protected FrameworkElement parentContainer;
        //пропускает показ 1ой анимации, может использоваться при одновременном проигрывании анимации в 2х регионах, когда 1й включает в себя 2й
        protected bool skipFirstAnimation = false;

        protected void EventHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            //RegionManager при навигации сначала устанавливает содержимое в null, затем новое значение, поэтому событие может возникать 2 раза подряд
            if (e.NewValue == null)
                return;

            if (skipFirstAnimation)
            { 
                skipFirstAnimation = false;
                return;
            }

            var elipseGeometry = new EllipseGeometry(Mouse.GetPosition(animatedElement), 0, 0);
            var point = Mouse.GetPosition(parentContainer);
            var x = Math.Max(point.X, parentContainer.ActualWidth - point.X);
            var y = Math.Max(point.Y, parentContainer.ActualHeight - point.Y);
            var radius = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            animatedElement.Clip = elipseGeometry;

            var duration = TimeSpan.FromMilliseconds(500);

            var ease = new ExponentialEase {
                EasingMode = EasingMode.EaseIn,
                Exponent = 1.5
            };

            var animationOpacity = new DoubleAnimation(0, 1, duration);
            animationOpacity.EasingFunction = ease;
            animatedElement.BeginAnimation(UIElement.OpacityProperty, animationOpacity);

            var animationX = new DoubleAnimation(0, radius, duration);
            animationX.EasingFunction = ease;
            elipseGeometry.BeginAnimation(EllipseGeometry.RadiusXProperty, animationX);

            var animationY = new DoubleAnimation(0, radius, duration);            
            animationY.EasingFunction = ease;
            animationY.Completed += (sndr, args) => animatedElement.Clip = null;
            elipseGeometry.BeginAnimation(EllipseGeometry.RadiusYProperty, animationY);
            
        }
    }
}
