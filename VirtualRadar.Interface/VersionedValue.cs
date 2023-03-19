// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Holds a value and the <see cref="IAircraft.DataVersion"/> that was current when the
    /// value was last changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VersionedValue<T>
    {
        /// <summary>
        /// The value that has been recorded.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// The <see cref="IAircraft.DataVersion"/> that was current when the value was last changed.
        /// If the value has never been set then this will be 0.
        /// </summary>
        public long DataVersion { get; private set; }

        /// <summary>
        /// Implicitly converts the struct to the value.
        /// </summary>
        /// <param name="versionedValue"></param>
        public static implicit operator T(VersionedValue<T> versionedValue) => versionedValue.Value;

        /// <summary>
        /// Creates a new value.
        /// </summary>
        public VersionedValue() : this(default, 0)
        {
        }

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <param name="initialDataVersion"></param>
        public VersionedValue(T initialValue, long initialDataVersion)
        {
            Value = initialValue;
            DataVersion = initialDataVersion;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value?.ToString();

        /// <summary>
        /// Assigns a new value unless the value has not changed.
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="newDataVersion">No assumptions are made about the version. It can be the same as, less than
        /// or greater than the existing value.</param>
        /// <returns>The <see cref="Value"/> after assignment.</returns>
        public bool SetValue(T newValue, long newDataVersion)
        {
            var result = !Object.Equals(Value, newValue);
            if(result) {
                Value = newValue;
                DataVersion = newDataVersion;
            }
            return result;
        }

        /// <summary>
        /// Returns true if the <see cref="DataVersion"/> is a larger number than the
        /// version passed across. Note that ascending order of version is *not* enforced
        /// by <see cref="SetValue"/>.
        /// </summary>
        /// <param name="sinceDataVersion"></param>
        /// <returns></returns>
        public bool IsAfter(long sinceDataVersion) => DataVersion > sinceDataVersion;

        /// <summary>
        /// Copies the value and version from the original.
        /// </summary>
        /// <param name="original"></param>
        public void CopyFrom(VersionedValue<T> original)
        {
            if(original == null) {
                throw new ArgumentNullException(nameof(original));
            }
            Value = original.Value;
            DataVersion = original.DataVersion;
        }
    }
}
