// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Reflection;
using System.Runtime.Serialization;

namespace Test.Framework
{
    /// <summary>
    /// Assertions ported from .NET Framework VRS's TestUtilities class.
    /// </summary>
    public static class AssertExtra
    {
        /// <summary>
        /// Throws an assertion if the type passed across is not sealed and is not based on Attribute.
        /// </summary>
        /// <param name="attributeType"></param>
        public static void IsAttribute(Type attributeType)
        {
            Assert.IsNotNull(attributeType);
            Assert.IsTrue(typeof(Attribute).IsAssignableFrom(attributeType), "Attribute does not inherit from System.Attribute");
            Assert.IsTrue(attributeType.IsSealed, "Attribute is not sealed");
            Assert.AreNotEqual(0, attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false).Length);
        }

        /// <summary>
        /// Throws an assertion if the type passed across is not an exception type, does not have the
        /// correct constructors or is not serialisable.
        /// </summary>
        /// <param name="exceptionType"></param>
        public static void IsException(Type exceptionType)
        {
            Assert.IsNotNull(exceptionType);
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(exceptionType), "Exception does not inherit from System.Exception");
            #if PocketPC || WindowsCE || NETCF
            #else
                if(exceptionType.GetCustomAttributes(typeof(SerializableAttribute), false) == null) {
                    Assert.Fail("The exception is not serializable");
                }
                ConstructorInfo serializeCtor = exceptionType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new Type[] {
                        typeof(SerializationInfo),
                        typeof(StreamingContext)
                    },
                    Array.Empty<ParameterModifier>()
                );
                Assert.IsNotNull(serializeCtor);
            #endif

            var defaultCtor = exceptionType.GetConstructor(new Type[] {});
            var stringCtor = exceptionType.GetConstructor(new Type[] {typeof(String)});
            var innerExceptionCtor = exceptionType.GetConstructor(new Type[] {typeof(String), typeof(Exception)});

            Assert.IsNotNull(defaultCtor == null);
            Assert.IsNotNull(stringCtor == null);
            Assert.IsNotNull(innerExceptionCtor == null);

            var defaultException = (Exception)Activator.CreateInstance(exceptionType);
            Assert.IsNull(defaultException.InnerException);

            var expectedText = "My Message TeXT";
            var stringException = (Exception)stringCtor.Invoke(new object[]{expectedText});
            Assert.IsTrue(stringException.Message.IndexOf(expectedText) > -1);
            Assert.IsNull(stringException.InnerException);

            var inner = new Exception();
            var innerException = (Exception)innerExceptionCtor.Invoke(new object[]{expectedText, inner});
            Assert.IsTrue(innerException.Message.IndexOf(expectedText) > -1);
            Assert.AreSame(inner, innerException.InnerException);
        }

        /// <summary>
        /// Asserts that either both sequences are null or both are the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void SequenceEqualOrNull<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null)
        {
            Assert.IsTrue(
                (expected == null && actual == null) || (expected != null && actual != null),
                message ?? $"expected is {(expected == null ? "" : "not ")}null and actual is {(actual == null ? "" : "not ")}null"
            );
            if(expected != null) {
                Assert.IsTrue(
                    expected.SequenceEqual(actual),
                    message ?? $"SequenceEqual is returning false when comparing expected to actual"
                );
            }
        }

        /// <summary>
        /// Compares two date times down to second precision. Differences in milliseconds are ignored.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="message"></param>
        public static void AreEqualToSeconds(DateTime lhs, DateTime rhs, string message = null)
        {
            Assert.AreEqual(
                TruncateDateTimeToSeconds(lhs),
                TruncateDateTimeToSeconds(rhs),
                message
            );
        }

        /// <summary>
        /// Compares two date times down to second precision. Differences in milliseconds are ignored.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="message"></param>
        public static void AreEqualToSeconds(DateTime? lhs, DateTime? rhs, string message = null)
        {
            if(lhs == null) {
                Assert.IsNull(rhs, message);
            } else if(rhs == null) {
                Assert.Fail(message);
            } else {
                Assert.AreEqual(
                    TruncateDateTimeToSeconds(lhs.Value),
                    TruncateDateTimeToSeconds(rhs.Value),
                    message
                );
            }
        }

        private static DateTime TruncateDateTimeToSeconds(DateTime original)
        {
            return new DateTime(
                original.Year, original.Month, original.Day,
                original.Hour, original.Minute, original.Second
            );
        }
    }
}
