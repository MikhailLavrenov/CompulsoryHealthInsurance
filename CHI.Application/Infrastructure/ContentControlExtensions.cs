using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CHI.Application.Infrastructure
{
    public static class ContentControlExtensions
    {
        public static readonly DependencyProperty ContentAnimation = DependencyProperty.RegisterAttached(
            nameof(ContentAnimation), 
            typeof(Storyboard), 
            typeof(ContentControlExtensions), 
            new PropertyMetadata(default(Storyboard), OnContentAnimationChanged));

        public static void SetContentAnimation(DependencyObject element, Storyboard value)
        {
            element.SetValue(ContentAnimation, value);
        }

        public static Storyboard GetContentAnimation(DependencyObject element)
        {
            return (Storyboard)element.GetValue(ContentAnimation);
        }

        private static void OnContentAnimationChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var contentControl = dependencyObject as ContentControl;

            if (contentControl == null)
                throw new Exception("Can only be applied to a ContentControl");

            var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl));

            propertyDescriptor.RemoveValueChanged(contentControl, ContentChangedHandler);
            propertyDescriptor.AddValueChanged(contentControl, ContentChangedHandler);
        }

        private static void ContentChangedHandler(object sender, EventArgs eventArgs)
        {
            var animateObject = (FrameworkElement)sender;
            var storyboard = GetContentAnimation(animateObject);
            storyboard.Begin(animateObject);
        }
    }
}
