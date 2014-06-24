using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for user controls that want to nominate which of their controls
    /// should have validation messages shown against it (as opposed to the validation
    /// message being shown against the user control itself).
    /// </summary>
    public interface IValidateDelegate
    {
        /// <summary>
        /// Returns the control to display validation messages against.
        /// </summary>
        /// <param name="errorProvider"></param>
        /// <returns></returns>
        Control GetValidationDisplayControl(ErrorProvider errorProvider);
    }
}
