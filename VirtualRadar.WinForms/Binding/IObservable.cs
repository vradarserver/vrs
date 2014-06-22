using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// The interface for observable values.
    /// </summary>
    public interface IObservable
    {
        /// <summary>
        /// Raised when the value is changed.
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns></returns>
        object GetValue();

        /// <summary>
        /// Sets the value and, if the value is a change, raises <see cref="Changed"/>.
        /// </summary>
        /// <param name="value"></param>
        void SetValue(object value);

        /// <summary>
        /// Sets the value and optionally suppresses the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="suppressEvents"></param>
        void SetValue(object value, bool suppressEvents);

        /// <summary>
        /// Returns the type of the value being bound.
        /// </summary>
        /// <returns></returns>
        Type GetValueType();
    }
}
