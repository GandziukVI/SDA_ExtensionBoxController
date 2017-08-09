using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using System.Windows;

namespace D3Helper
{
    public class ViewportAxesRangeRestriction : ViewportRestriction
    {
        public DisplayRange XRange = null;
        public DisplayRange YRange = null;

        public override DataRect Apply(DataRect oldVisible, DataRect newVisible, Viewport2D viewport)
        {
            if (XRange != null)
            {
                newVisible.XMin = XRange.Start;
                newVisible.Width = XRange.End - XRange.Start;
            }

            if (YRange != null)
            {
                newVisible.YMin = YRange.Start;
                newVisible.Height = YRange.End - YRange.Start;
            }

            return newVisible;
        }
    }
}
