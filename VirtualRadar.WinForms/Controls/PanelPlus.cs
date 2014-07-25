using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// Extends Panel to add support for things like double buffering.
    /// </summary>
    public class PanelPlus : Panel
    {
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PanelPlus() : base()
        {
            DoubleBuffered = true;
        }
    }
}
