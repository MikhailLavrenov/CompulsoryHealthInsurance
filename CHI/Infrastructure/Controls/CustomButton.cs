using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace CHI.Infrastructure
{
    public class CustomButton : Button
    {
        // свойство зависимостей
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
                        "Text",
                        typeof(string),
                        typeof(CustomButton),                                
                        new FrameworkPropertyMetadata(                        
                            string.Empty,                        
                            FrameworkPropertyMetadataOptions.AffectsMeasure |                        
                            FrameworkPropertyMetadataOptions.AffectsRender|                                     
                            FrameworkPropertyMetadataOptions.Inherits
                        ));

        public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(
                "IconKind",
                typeof(PackIconKind),
                typeof(CustomButton));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public PackIconKind IconKind
        {
            get { return (PackIconKind)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

    }
}
