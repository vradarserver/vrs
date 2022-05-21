using System.Collections.Generic;
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Handles the web addresses file in the configuration folder. Implementations are thread safe.
    /// </summary>
    [Singleton]
    public interface IWebAddressManager
    {
        /// <summary>
        /// The full path to the web addresses file.
        /// </summary>
        string AddressFileFullPath { get; }

        /// <summary>
        /// Adds or overwrites an address without overwriting existing custom addresses.
        /// </summary>
        /// <param name="name">The name of the address to add.</param>
        /// <param name="address">The address to add.</param>
        /// <param name="oldAddresses">An optional list of historical
        /// addresses that are to be overwritten with the new address. If an address
        /// already exists for <paramref name="name"/> and it is neither <paramref name="address"/>
        /// nor is it in the list of <paramref name="oldAddresses"/> then it is considered a
        /// custom address entered by the user and it is left unchanged.</param>
        /// <returns>The address actually registered against the name.</returns>
        string RegisterAddress(string name, string address, IList<string> oldAddresses = null);

        /// <summary>
        /// Returns the address associated with the case insensitive name. The program reserves
        /// names that start with 'vrs-'. Returns null if the name does not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string LookupAddress(string name);
    }
}
