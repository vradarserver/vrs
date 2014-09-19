using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface that supports a <see cref="ITimestampedExceptionView"/>.
    /// </summary>
    public interface ITimestampedExceptionPresenter : IPresenter<ITimestampedExceptionView>
    {
        /// <summary>
        /// Returns the time held by the exception as a localised string.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        string FormatTime(TimestampedException ex);

        /// <summary>
        /// Returns a formatted exception message. Newlines are represented by \\n.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        string FullExceptionMessage(TimestampedException ex);
    }
}
