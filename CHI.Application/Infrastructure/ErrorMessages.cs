using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Application.Infrastructure
{
    public static class ErrorMessages
    {
        public static string IsNullOrEmpty { get; } = "Значение не может быть пустым";
        public static string Connection { get; } = "Не удалось подключиться";
        public static string Authorization { get; } = "Не удалось авторизоваться";
        public static string LessOne { get; } = "Значение не может быть меньше 1";
        public static string UriFormat { get; } = "Не верный формат URI";
    }
}
