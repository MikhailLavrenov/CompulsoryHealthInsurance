﻿using System.Windows;
using System.Windows.Controls;

namespace PatientsFomsRepository.Infrastructure
{
    //Для реализации Binding в PasswordBox
    public static class PasswordBoxHelper
    {
        #region Свойства
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));
        public static readonly DependencyProperty BindPassword =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, OnBindPasswordChanged));
        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));
        #endregion

        #region Методы
        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }
        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }
        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }
        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }
        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }
        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as PasswordBox;
            //Обрабатываем событие только если свойство прикреплено к PassowrdBox и BindPassword установлено в  true
            if (d == null || !GetBindPassword(d))
                return;
            //предотвращает рекурсивный вызов путем игнорирования события изменения PasswordBox
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = (string)e.NewValue;

            if (!GetUpdatingPassword(box))
                box.Password = newPassword;

            box.PasswordChanged += HandlePasswordChanged;
        }
        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            //Когда прикрепленное свойство BindPassword установлено на PasswordBox начинаем прослушивать событие PasswordChanged
            var box = dp as PasswordBox;

            if (box == null)
                return;

            bool wasBound = (bool)(e.OldValue);
            bool needToBind = (bool)(e.NewValue);

            if (wasBound)
                box.PasswordChanged -= HandlePasswordChanged;

            if (needToBind)
                box.PasswordChanged += HandlePasswordChanged;
        }
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = sender as PasswordBox;

            // устанавливает флаг, что пароль обновлен
            SetUpdatingPassword(box, true);
            // отправляет новый пароль в прикрепленное свойство  BoundPassword 
            SetBoundPassword(box, box.Password);
            SetUpdatingPassword(box, false);
        }
        #endregion
    }
}
