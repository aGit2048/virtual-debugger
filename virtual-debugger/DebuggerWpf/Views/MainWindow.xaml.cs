using CustomControlLibrary;
using DebuggerWpf.Components;
using DebuggerWpf.Models;
using DebuggerWpf.MqttUtility;
using DebuggerWpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DebuggerWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private ControllerDataTableViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new ControllerDataTableViewModel();
            _viewModel.IsConsolePanelShowControllerInfo = ConsolePanel.IsShowControllerInfo;
            this.DataContext = _viewModel;
        }

        private void OnClick_GenerateMsgCode(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ControllerDataTableItem item = (ControllerDataTableItem)button.Tag;
            MqttMsgGenerator.Generate(this, item);
        }

        private void OnDoubleClick_ControlPanelTitle(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            RowDefinition consoleRow = WindowMainGrid.RowDefinitions[3];
            double actualHeight = consoleRow.ActualHeight;

            if (actualHeight == consoleRow.MaxHeight)
            {
                consoleRow.Height = new GridLength(consoleRow.MinHeight);
            }
            else
            {
                consoleRow.Height = new GridLength(consoleRow.MaxHeight);
            }
        }

        private void OnClick_Clear(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is Button btn)) return;
            _viewModel.ClearConsoleData();
        }

        private void OnShowControllerInfo_CheckValueChanged(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is LabelToggleButton toggleButton)) return;
            _viewModel.ConsoleLogCheckControllerInfo(toggleButton.IsChecked == true);
        }

        private void OnClick_CopyListItem(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is BasicButton basicButton)) return;
            ConsoleLogItem logItem = basicButton.Tag as ConsoleLogItem;
            _viewModel.ConsoleLogCopy(logItem, ConsolePanel.IsCopyAll);
        }

        private void OnClick_SendMqttMsg(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ControllerDataTableItem item = (ControllerDataTableItem)button.Tag;
            string msgString = MqttMsgGenerator.Generate(this, item);
            _ = MqttClientManager.PushMsgAsync(item.Topic, msgString);
        }

        private void OnClick_EditItem(object sender, RoutedEventArgs e)
        {
            var textContent = new TextBlock
            {
                Text = "这是一个简单的模态对话框",
                Foreground = Brushes.White,
                FontSize = 16,
                Background = Brushes.Black,
                Padding = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var modalWindow = new ModalWindow(this, textContent);
            modalWindow.ShowDialog();
        }
    }
}
