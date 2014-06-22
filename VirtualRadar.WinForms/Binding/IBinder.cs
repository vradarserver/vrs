using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// The interface for all objects that can bind properties to controls.
    /// </summary>
    public interface IBinder : IDisposable
    {
        /// <summary>
        /// Gets the observable value.
        /// </summary>
        IObservable Observable { get; }

        /// <summary>
        /// Gets the bound control.
        /// </summary>
        Control Control { get; }

        /// <summary>
        /// Copies the current value to the control without raising any changed events.
        /// </summary>
        void InitialiseControl();

        /// <summary>
        /// Forces a refresh of the control's content.
        /// </summary>
        void RefreshControl();
    }
}
