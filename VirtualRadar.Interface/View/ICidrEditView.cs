using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for views that let the user enter CIDRs.
    /// </summary>
    public interface ICidrEditView : IView
    {
        /// <summary>
        /// Gets or sets the text of the CIDR.
        /// </summary>
        string Cidr { get; set; }

        /// <summary>
        /// Gets a value indicating that the value of <see cref="Cidr"/> represents a valid CIDR.
        /// </summary>
        bool CidrIsValid { get; }

        /// <summary>
        /// Gets or sets a description that shows the first matching address.
        /// </summary>
        string FirstMatchingAddress { get; set; }

        /// <summary>
        /// Gets or sets a description that shows the last matching address.
        /// </summary>
        string LastMatchingAddress { get; set; }

        /// <summary>
        /// Raised when the <see cref="Cidr"/> has changed.
        /// </summary>
        event EventHandler CidrChanged;
    }
}
