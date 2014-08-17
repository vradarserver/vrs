using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Compares <see cref="Cidr"/> objects for their relative order.
    /// </summary>
    /// <remarks>
    /// The CIDRs are sorted in mask order and then in ascending bit count order.
    /// </remarks>
    public class CidrComparer : IComparer<Cidr>
    {
        /// <summary>
        /// The object we use to compare IP addresses.
        /// </summary>
        private IPAddressComparer _IPAddressComparer = new IPAddressComparer();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public int Compare(Cidr lhs, Cidr rhs)
        {
            int result = Object.ReferenceEquals(lhs, rhs) ? 0 : -1;
            if(result != 0) {
                if(lhs == null && rhs == null) result = 0;
                else if(lhs == null) result = -1;
                else if(rhs == null) result = 1;
                else {
                    result = _IPAddressComparer.Compare(lhs.MaskedAddress, rhs.MaskedAddress);
                    if(result == 0) result = lhs.BitmaskBits - rhs.BitmaskBits;
                }
            }

            return result;
        }
    }
}
