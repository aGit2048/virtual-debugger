using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomControlLibrary
{
    public class BasicButton : Button
    {
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(BasicButton), new PropertyMetadata(null));


        /// <summary>
        /// 注册双击路由事件
        /// </summary>
        public static readonly RoutedEvent DoubleClickEvent =
            EventManager.RegisterRoutedEvent(
                "DoubleClick",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(BasicButton));

        /// <summary>
        /// 双击事件CLR包装器
        /// </summary>
        public event RoutedEventHandler DoubleClick
        {
            add { AddHandler(DoubleClickEvent, value); }
            remove { RemoveHandler(DoubleClickEvent, value); }
        }

        /// <summary>
        /// 双击时间间隔（毫秒）
        /// </summary>
        public static readonly DependencyProperty DoubleClickIntervalProperty =
            DependencyProperty.Register(
                "DoubleClickInterval",
                typeof(int),
                typeof(BasicButton),
                new PropertyMetadata(500));

        public int DoubleClickInterval
        {
            get { return (int)GetValue(DoubleClickIntervalProperty); }
            set { SetValue(DoubleClickIntervalProperty, value); }
        }

        private DateTime _lastClickTime = DateTime.MinValue;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var currentTime = DateTime.Now;
            var timeSinceLastClick = currentTime - _lastClickTime;

            if (timeSinceLastClick.TotalMilliseconds < DoubleClickInterval)
            {
                // 触发双击事件
                RaiseDoubleClickEvent();
                _lastClickTime = DateTime.MinValue; // 重置
                e.Handled = true; // 标记为已处理
            }
            else
            {
                _lastClickTime = currentTime;
            }
        }

        // 触发双击事件的方法
        protected virtual void RaiseDoubleClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DoubleClickEvent);
            RaiseEvent(newEventArgs);
        }


        static BasicButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicButton), new FrameworkPropertyMetadata(typeof(BasicButton)));
        }
    }
}
