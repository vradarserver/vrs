using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Presenter
{
    /// <summary>
    /// The interface for objects that provide the business logic for
    /// <see cref="ISettingsView"/> views.
    /// </summary>
    public interface ISettingsPresenter : IPresenter<ISettingsView>, IDisposable
    {
        /// <summary>
        /// Gets or sets the object that abstracts away the environment for the presenter.
        /// </summary>
        ISettingsPresenterProvider Provider { get; set; }

        /// <summary>
        /// Creates a new merged feed. The object is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        MergedFeed CreateMergedFeed();

        /// <summary>
        /// Creates a new rebroadcast server. The object is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        RebroadcastSettings CreateRebroadcastServer();

        /// <summary>
        /// Creates a new receiver. The receiver is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        Receiver CreateReceiver();

        /// <summary>
        /// Creates a new receiver location. The receiver location is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        ReceiverLocation CreateReceiverLocation();

        /// <summary>
        /// Creates a new user. The user is not attached to the configuration being edited.
        /// </summary>
        /// <returns></returns>
        IUser CreateUser();

        /// <summary>
        /// Returns a list of serial port names. Guaranteed not to throw an exception and to be current (i.e. no caching).
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSerialPortNames();

        /// <summary>
        /// Applies the answers from a receiver configuration wizard to the receiver passed across.
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="receiver"></param>
        void ApplyReceiverConfigurationWizard(IReceiverConfigurationWizardAnswers answers, Receiver receiver);
    }
}
