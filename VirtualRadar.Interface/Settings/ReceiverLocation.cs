// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Settings
{
    #pragma warning disable 0659 // Disable warning about overriding Equals but not GetHashCode - I don't want this used as a key, it's mutable.
    /// <summary>
    /// A class describing the location of a receiver.
    /// </summary>
    [Serializable]
    public class ReceiverLocation : ICloneable
    {
        /// <summary>
        /// Gets or sets the unique internal identifier of the receiver location.
        /// </summary>
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the location's latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the location's longitude.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the location was copied from the BaseStation database.
        /// </summary>
        public bool IsBaseStationLocation { get; set; }

        /// <summary>
        /// Returns an English description of the location.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name ?? "";

        /// <summary>
        /// See base docs. Note that GetHashCode is not overridden, this class is mutable and not safe for use as a key.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            bool result = Object.ReferenceEquals(this, obj);
            if(!result && obj is ReceiverLocation other) {
                result = other.IsBaseStationLocation == IsBaseStationLocation &&
                         other.Latitude == Latitude &&
                         other.Longitude == Longitude &&
                         other.Name == Name &&
                         other.UniqueId == UniqueId;
            }

            return result;
        }

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var result = (ReceiverLocation)Activator.CreateInstance(GetType());
            result.IsBaseStationLocation = IsBaseStationLocation;
            result.Latitude = Latitude;
            result.Longitude = Longitude;
            result.Name = Name;
            result.UniqueId = UniqueId;

            return result;
        }
    }
    #pragma warning restore 0659
}
