using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that provide the business logic for
    /// <see cref="ISettingsView"/> views.
    /// </summary>
    public interface ISettingsPresenter : IPresenter<ISettingsView>
    {
    }
}
