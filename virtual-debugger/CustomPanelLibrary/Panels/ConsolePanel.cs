using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CustomPanelLibrary
{
    public class ConsolePanel : Control
    {
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ConsolePanel), new PropertyMetadata(null));



        static ConsolePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConsolePanel), new FrameworkPropertyMetadata(typeof(ConsolePanel)));
        }
    }
}
