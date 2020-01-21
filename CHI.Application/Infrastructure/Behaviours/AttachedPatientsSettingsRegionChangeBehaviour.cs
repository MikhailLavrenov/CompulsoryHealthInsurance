using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CHI.Application.Infrastructure
{

    /// <summary>
    /// Анимация смены представления в AttachedPatientsSettingsRegion
    /// </summary>
    public class AttachedPatientsSettingsRegionChangeBehaviour : CircleAnimationBaseBehaviour
    {
        protected override void OnAttached()
        {
            customContentControl = AssociatedObject.FindLogicalParent<CustomContentControl>();
            customContentControl.ContentChanged += EventHandler;

            animatedElement = customContentControl;
            parentContainer = AssociatedObject.FindLogicalParent<DockPanel>();            
        }
        protected override void OnDetaching()
        {
            customContentControl.ContentChanged -= EventHandler;
        }
    }
}
