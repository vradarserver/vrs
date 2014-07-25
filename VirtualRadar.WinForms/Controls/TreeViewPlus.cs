using InterfaceFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interop;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A double-buffering tree-view.
    /// </summary>
    public class TreeViewPlus : TreeView
    {
        // TreeView messages
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;

        // TreeView extended styles
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        /// <summary>
        /// Sets the extended styles on the tree view.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            if(!DesignMode) {
                Window.CallSendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            }
            base.OnHandleCreated(e);
        }
    }
}
