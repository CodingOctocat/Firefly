using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Firefly.Controls;

public class TopPlacementComboBox : ComboBox
{
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var popup = (Popup)Template.FindName("PART_Popup", this);
        popup.Placement = PlacementMode.Top;
    }
}
