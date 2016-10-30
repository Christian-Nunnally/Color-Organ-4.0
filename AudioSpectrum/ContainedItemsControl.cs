using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum
{
    public class ContainedItemsControl : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return false;
        }
    }
}
