using System.Windows;
using System.Windows.Controls.Primitives;

namespace CustomControlLibrary
{
    public class LabelToggleButton : ToggleButton
    {
        static LabelToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelToggleButton), new FrameworkPropertyMetadata(typeof(LabelToggleButton)));
        }
    }
}
