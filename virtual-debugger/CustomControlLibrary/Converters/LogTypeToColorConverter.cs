using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomControlLibrary.Converters
{
    internal class LogTypeToColorConverter : IValueConverter
    {
        private static LogTypeToColorConverter _instance = new LogTypeToColorConverter();
        public static LogTypeToColorConverter Instance => _instance;

        private LogTypeToColorConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = value.ToString().ToLower();
            switch (result)
            {
                case "error":
                    return new SolidColorBrush(Colors.Red);
                case "info":
                    return new SolidColorBrush(Colors.LightBlue);
                case "debug":
                    return new SolidColorBrush(Colors.Yellow);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
