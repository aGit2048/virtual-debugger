using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml.Linq;

namespace CustomControlLibrary
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

        public bool IsShowControllerInfo
        {
            get { return (bool)GetValue(IsShowControllerInfoProperty); }
            set { SetValue(IsShowControllerInfoProperty, value); }
        }
        public static readonly DependencyProperty IsShowControllerInfoProperty =
            DependencyProperty.Register("IsShowControllerInfo", typeof(bool), typeof(ConsolePanel), new PropertyMetadata(false));

        public bool IsCopyAll
        {
            get { return (bool)GetValue(IsCopyAllProperty); }
            set { SetValue(IsCopyAllProperty, value); }
        }
        public static readonly DependencyProperty IsCopyAllProperty =
            DependencyProperty.Register("IsCopyAll", typeof(bool), typeof(ConsolePanel), new PropertyMetadata(false));

        public static readonly RoutedEvent ClearClickEvent =
            EventManager.RegisterRoutedEvent("Clear", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConsolePanel));
        public event RoutedEventHandler ClearClick
        {
            add { AddHandler(ClearClickEvent, value); }
            remove { RemoveHandler(ClearClickEvent, value); }
        }

        public static readonly RoutedEvent ShowControllerInfoCheckEvent =
           EventManager.RegisterRoutedEvent("ShowControllerInfoCheck", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConsolePanel));
        public event RoutedEventHandler ShowControllerInfoCheck
        {
            add { AddHandler(ShowControllerInfoCheckEvent, value); }
            remove { RemoveHandler(ShowControllerInfoCheckEvent, value); }
        }

        public static readonly RoutedEvent CopyListItemEvent =
           EventManager.RegisterRoutedEvent("CopyListItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConsolePanel));
        public event RoutedEventHandler CopyListItem
        {
            add { AddHandler(CopyListItemEvent, value); }
            remove { RemoveHandler(CopyListItemEvent, value); }
        }

        private ListBox _loggerContainer;

        static ConsolePanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConsolePanel), new FrameworkPropertyMetadata(typeof(ConsolePanel)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button clearButton = GetTemplateChild("PART_ClearButton") as Button;
            if (clearButton != null)
            {
                clearButton.Click += OnClick_Clear;
            }
            LabelToggleButton showControllerInfoToggle = GetTemplateChild("PART_ShowControllerInfoToggle") as LabelToggleButton;
            if (showControllerInfoToggle != null)
            {
                showControllerInfoToggle.Checked += OnShowControllerInfoToggle_CheckChanged;
                showControllerInfoToggle.Unchecked += OnShowControllerInfoToggle_CheckChanged;
            }
            _loggerContainer = GetTemplateChild("PART_LoggerContainer") as ListBox;
            if (_loggerContainer != null)
            {
                _loggerContainer.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            }
        }

        private void OnClick_Clear(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ClearClickEvent, sender);
            RaiseEvent(newEventArgs);
        }

        private void OnShowControllerInfoToggle_CheckChanged(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ShowControllerInfoCheckEvent, sender);
            RaiseEvent(newEventArgs);
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // 新项添加后，等待容器生成
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    int index = _loggerContainer.Items.Count - 1;       //注意：Items是数据项，不是ListBoxItems
                    EnsureButtonBinding(index);

                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }


        private void EnsureButtonBinding(int index)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ListBoxItem container = _loggerContainer.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;

                if (container == null)
                {
                    // 容器不存在，等待生成
                    WaitForContainerGeneration(index);
                    return;
                }

                if (container.IsLoaded)
                {
                    // 容器已加载，立即处理
                    TryBindButton(container);
                }
                else
                {
                    // 容器未加载，等待Loaded事件
                    container.Loaded += (s, e) => TryBindButton(container);
                }

            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void WaitForContainerGeneration(int index)
        {
            _loggerContainer.ItemContainerGenerator.StatusChanged -= OnStatusChanged;
            _loggerContainer.ItemContainerGenerator.StatusChanged += OnStatusChanged;

            void OnStatusChanged(object sender, EventArgs e)
            {
                if (_loggerContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    _loggerContainer.ItemContainerGenerator.StatusChanged -= OnStatusChanged;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        EnsureButtonBinding(index);
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
        }
        private void TryBindButton(ListBoxItem container)
        {
            BasicButton button = FindVisualChild<BasicButton>(container);

            if (button != null)
            {
                button.Click -= OnClick_CopyButton;
                button.Click += OnClick_CopyButton;
            }
            else
            {
                // 终极重试：使用Render优先级
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    button = FindVisualChild<BasicButton>(container);
                    if (button != null)
                    {
                        button.Click -= OnClick_CopyButton;
                        button.Click += OnClick_CopyButton;
                    }
                    else
                    {
                        Console.WriteLine("最终仍未找到按钮，可能模板结构不同");
                    }
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }


        private void OnClick_CopyButton(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CopyListItemEvent, sender);
            RaiseEvent(newEventArgs);
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}
