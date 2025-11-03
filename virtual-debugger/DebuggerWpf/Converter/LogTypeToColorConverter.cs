using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DebuggerWpf.Converter
{
    internal class LogTypeToColorConverter : IValueConverter
    {
        public static LogTypeToColorConverter Instance { get; } = new LogTypeToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string logType)
            {
                switch (logType.ToLower())
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
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
