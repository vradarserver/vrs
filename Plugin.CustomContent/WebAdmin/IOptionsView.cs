using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.CustomContent.WebAdmin
{
    public interface IOptionsView : IView
    {
        /// <summary>
        /// Returns the current option settings.
        /// </summary>
        /// <returns></returns>
        ViewModel GetState();

        /// <summary>
        /// Saves the settings passed across.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        SaveOutcomeModel Save(ViewModel viewModel);
    }
}
