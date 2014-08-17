using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that hold the business logic behind CIDR editors.
    /// </summary>
    public interface ICidrEditPresenter : IPresenter<ICidrEditView>
    {
        /// <summary>
        /// Gets a value indicating that the CIDR on the view is valid.
        /// </summary>
        bool CidrIsValid { get; }
    }
}
