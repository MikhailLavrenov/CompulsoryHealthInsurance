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
    /// Анимация смены представления в MainRegion
    /// </summary>
    public class MainRegionChangeBehaviour : CircleAnimationBaseBehaviour
    {

        protected override void OnAttached()
        {
            customContentControl = AssociatedObject.FindLogicalParent<CustomContentControl>();
            customContentControl.ContentChanged += EventHandler;

            animatedElement = AssociatedObject.FindLogicalParent<DockPanel>();
            parentContainer = AssociatedObject.FindLogicalParent<Window>();

        }
        protected override void OnDetaching()
        {
            customContentControl.ContentChanged -= EventHandler;
        }
    }
}
