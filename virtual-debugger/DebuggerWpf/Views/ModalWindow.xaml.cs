using System;
using System.Windows;
using System.Windows.Media;

namespace DebuggerWpf
{
    /// <summary>
    /// ModalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModalWindow : Window
    {
        public ModalWindow(Window owner, UIElement content)
        {
            Owner = owner;
            InitializeComponent();

            ContentContainer.Content = content;

            // 窗口设置
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;

            // 初始匹配
            PerfectMatchOwner();

            // 监听所有可能影响位置和尺寸的事件
            if (owner != null)
            {
                owner.SizeChanged += Owner_SizeChanged;
                owner.LocationChanged += Owner_LocationChanged;
                owner.StateChanged += Owner_StateChanged;
                owner.Activated += Owner_Activated;
                owner.Deactivated += Owner_Deactivated;

                // 也监听自己的变化
                SizeChanged += ModalWindow_SizeChanged;
                LocationChanged += ModalWindow_LocationChanged;
            }
        }

        private void PerfectMatchOwner()
        {
            if (Owner == null) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 确保在UI线程完成所有布局后执行
                    Width = Owner.ActualWidth;
                    Height = Owner.ActualHeight;
                    Left = Owner.Left;
                    Top = Owner.Top;
                    WindowState = Owner.WindowState;
                    // 强制刷新布局
                    InvalidateMeasure();
                    InvalidateArrange();
                    UpdateLayout();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"匹配父窗口尺寸时出错: {ex.Message}");
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void Owner_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PerfectMatchOwner();
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            PerfectMatchOwner();
        }

        private void Owner_StateChanged(object sender, EventArgs e)
        {
            PerfectMatchOwner();
        }

        private void Owner_Activated(object sender, EventArgs e)
        {
            PerfectMatchOwner();
        }

        private void Owner_Deactivated(object sender, EventArgs e)
        {
            PerfectMatchOwner();
        }

        private void ModalWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PerfectMatchOwner();
        }

        private void ModalWindow_LocationChanged(object sender, EventArgs e)
        {
            PerfectMatchOwner();
        }

        protected override void OnClosed(EventArgs e)
        {
            // 清理所有事件监听
            if (Owner != null)
            {
                Owner.SizeChanged -= Owner_SizeChanged;
                Owner.LocationChanged -= Owner_LocationChanged;
                Owner.StateChanged -= Owner_StateChanged;
                Owner.Activated -= Owner_Activated;
                Owner.Deactivated -= Owner_Deactivated;
            }

            SizeChanged -= ModalWindow_SizeChanged;
            LocationChanged -= ModalWindow_LocationChanged;

            base.OnClosed(e);
            Owner?.Focus();
        }
    }
}
