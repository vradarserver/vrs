using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface that Receiver Configuration wizards need to implement.
    /// </summary>
    public interface IReceiverConfigurationWizard : IView
    {
        /// <summary>
        /// Gets or sets the user's answers to the wizard's questions.
        /// </summary>
        IReceiverConfigurationWizardAnswers Answers { get; set; }
    }
}
