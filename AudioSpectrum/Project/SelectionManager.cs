using System.Collections.Generic;
using AudioSpectrum.RackItems;

namespace AudioSpectrum
{
    public class SelectionManager
    {
        public static readonly SelectionManager SelectionMgr = new SelectionManager();

        private readonly List<ISelectable> _selectedItems = new List<ISelectable>();
        public SetSideRailDelegate SetSideRail;

        private SelectionManager()
        {
        }

        public void Select(ISelectable selectable)
        {
            foreach (var selectedItem in _selectedItems)
                selectedItem.IsSelected = false;

            _selectedItems.Clear();

            selectable.IsSelected = true;
            _selectedItems.Add(selectable);
            SpecialSelectRules(selectable);
        }

        private void SpecialSelectRules(ISelectable selectable)
        {
            var rackItem = selectable as IRackItem;
            if ((rackItem != null) && (SetSideRail != null))
                rackItem.SetSideRail(SetSideRail);
        }
    }
}