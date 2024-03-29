﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CHI.Infrastructure
{
    //Присоединненное свойство для привязки пароля в PasswordBox
    public static class PasswordBoxHelper
    {
        /// <summary>
        /// Свойство зависимостей связанное с паролем PasswordBox
        /// </summary>
        public static readonly DependencyProperty BoundPassword = DependencyProperty.RegisterAttached(
            nameof(BoundPassword),
            typeof(string),
            typeof(PasswordBoxHelper),
             new FrameworkPropertyMetadata(null, OnBoundPasswordChanged)
             {
                 BindsTwoWayByDefault = true,
                 DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
             });


        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }
        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = d as PasswordBox;

            if (passwordBox == null)
                return;

            // исключает рекурсивный вызов и повторное подписывание на событие
            passwordBox.PasswordChanged -= OnPasswordChanged;

            var newPassword = (string)e.NewValue;

            if (passwordBox.Password != newPassword)
                passwordBox.Password = newPassword;

            passwordBox.PasswordChanged += OnPasswordChanged;
        }
        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            SetBoundPassword(passwordBox, passwordBox.Password);
        }
    }
}
