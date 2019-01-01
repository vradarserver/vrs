// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Moq;
using System.Linq.Expressions;
using InterfaceFactory;
using VirtualRadar.Interface;
using System.Collections.Specialized;
using System.IO;

namespace Test.Framework
{
    /// <summary>
    /// A collection of static methods that cover common aspects of testing objects.
    /// </summary>
    public static class TestUtilities
    {
        #region AssertIsAttribute
        /// <summary>
        /// Throws an assertion if the type passed across is not sealed and is not based on Attribute.
        /// </summary>
        /// <param name="attributeType"></param>
        public static void AssertIsAttribute(Type attributeType)
        {
            Assert.IsNotNull(attributeType);
            Assert.IsTrue(typeof(Attribute).IsAssignableFrom(attributeType), "Attribute does not inherit from System.Attribute");
            Assert.IsTrue(attributeType.IsSealed, "Attribute is not sealed");
            Assert.AreNotEqual(0, attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false).Length);
        }
        #endregion

        #region AssertIsException
        /// <summary>
        /// Throws an assertion if the type passed across is not an exception type, does not have the
        /// correct constructors or is not serialisable.
        /// </summary>
        /// <param name="exceptionType"></param>
        public static void AssertIsException(Type exceptionType)
        {
            Assert.IsNotNull(exceptionType);
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(exceptionType), "Exception does not inherit from System.Exception");
            #if PocketPC || WindowsCE || NETCF
            #else
                if(exceptionType.GetCustomAttributes(typeof(SerializableAttribute), false) == null) Assert.Fail("The exception is not serializable");
                ConstructorInfo serializeCtor = exceptionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof(SerializationInfo), typeof(StreamingContext)}, new ParameterModifier[]{});
                Assert.IsNotNull(serializeCtor);
            #endif

            ConstructorInfo defaultCtor = exceptionType.GetConstructor(new Type[] {});
            ConstructorInfo stringCtor = exceptionType.GetConstructor(new Type[] {typeof(String)});
            ConstructorInfo innerExceptionCtor = exceptionType.GetConstructor(new Type[] {typeof(String), typeof(Exception)});

            Assert.IsNotNull(defaultCtor == null);
            Assert.IsNotNull(stringCtor == null);
            Assert.IsNotNull(innerExceptionCtor == null);

            Exception defaultException = (Exception)Activator.CreateInstance(exceptionType);
            Assert.IsNull(defaultException.InnerException);

            string expectedText = "My Message TeXT";
            Exception stringException = (Exception)stringCtor.Invoke(new object[]{expectedText});
            Assert.IsTrue(stringException.Message.IndexOf(expectedText) > -1);
            Assert.IsNull(stringException.InnerException);

            Exception inner = new Exception();
            Exception innerException = (Exception)innerExceptionCtor.Invoke(new object[]{expectedText, inner});
            Assert.IsTrue(innerException.Message.IndexOf(expectedText) > -1);
            Assert.AreSame(inner, innerException.InnerException);
        }
        #endregion

        #region TestProperty
        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to another value.
        /// </summary>
        /// <param name="obj">The object under test.</param>
        /// <param name="propertyName">The name of the property to test.</param>
        /// <param name="startValue">The expected starting value of the property.</param>
        /// <param name="newValue">The value to write to the property - to pass the property must expose this value once it has been set.</param>
        /// <param name="testForEquals">True if the equality test uses <see cref="object.Equals(object)"/>, false if it uses <see cref="Object.ReferenceEquals"/>.
        /// If either the startValue or newValue are value types then this parameter is ignored, the test is always made using <see cref="object.Equals(object)"/>.</param>
        public static void TestProperty(object obj, string propertyName, object startValue, object newValue, bool testForEquals)
        {
            Assert.IsNotNull(obj);
            PropertyInfo property = obj.GetType().GetProperty(propertyName);
            Assert.IsNotNull(property, "{0}.{1} is not a public property", obj.GetType().Name, propertyName);

            if(startValue != null && startValue.GetType().IsValueType) testForEquals = true;
            if(newValue != null && newValue.GetType().IsValueType) testForEquals = true;

            object actualStart = property.GetValue(obj, null);
            if(testForEquals) Assert.AreEqual(startValue, actualStart, "{0}.{1} expected [{2}] was [{3}]", obj.GetType().Name, propertyName, startValue == null ? "null" : startValue.ToString(), actualStart == null ? "null" : actualStart.ToString());
            else              Assert.AreSame(startValue, actualStart, "{0}.{1} expected [{2}] was [{3}]", obj.GetType().Name, propertyName, startValue == null ? "null" : startValue.ToString(), actualStart == null ? "null" : actualStart.ToString());

            if(property.CanWrite) {
                property.SetValue(obj, newValue, null);
                object actualNew = property.GetValue(obj, null);
                if(testForEquals) Assert.AreEqual(newValue, actualNew, "{0}.{1} was set to [{2}] but it returned [{3}] when read", obj.GetType().Name, propertyName, newValue == null ? "null" : newValue.ToString(), actualNew == null ? "null" : actualNew.ToString());
                else              Assert.AreSame(newValue, actualNew, "{0}.{1} was set to [{2}] but it returned [{3}] when read", obj.GetType().Name, propertyName, newValue == null ? "null" : newValue.ToString(), actualNew == null ? "null" : actualNew.ToString());
            }
        }

        /// <summary>
        /// Flips the case of the string passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FlipCase(string text)
        {
            var result = text;

            if(!String.IsNullOrEmpty(result)) {
                var buffer = new StringBuilder(result);
                for(var i = 0;i < buffer.Length;++i) {
                    var ch = buffer[i];
                    buffer[i] = Char.IsUpper(ch) ? Char.ToLower(ch) : Char.ToUpper(ch);
                }
                result = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// Asserts that the property passed across starts at a given value and can be changed to another value.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        /// <param name="newValue"></param>
        /// <param name="testForEquals"></param>
        public static void TestProperty<T>(T obj, Expression<Func<T, object>> propertyExpression, object startValue, object newValue, bool testForEquals)
        {
            TestProperty(obj, PropertyHelper.ExtractName(propertyExpression), startValue, newValue, testForEquals);
        }

        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to another value.
        /// </summary>
        /// <param name="obj">The object under test.</param>
        /// <param name="propertyName">The name of the property to test.</param>
        /// <param name="startValue">The expected starting value of the property.</param>
        /// <param name="newValue">The value to write to the property - to pass the property must expose this value once it has been set.</param>
        public static void TestProperty(object obj, string propertyName, object startValue, object newValue)
        {
            TestProperty(obj, propertyName, startValue, newValue, false);
        }

        /// <summary>
        /// Asserts that a property of an object has a given start value and can be successfuly changed to another value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        /// <param name="newValue"></param>
        public static void TestProperty<T>(T obj, Expression<Func<T, object>> propertyExpression, object startValue, object newValue)
        {
            TestProperty(obj, PropertyHelper.ExtractName(propertyExpression), startValue, newValue, false);
        }

        /// <summary>
        /// Asserts that a bool property of an object has the given start value. Sets the property to the toggled start value and asserts that
        /// the toggled value can then be read back from the property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="startValue"></param>
        public static void TestProperty(object obj, string propertyName, bool startValue)
        {
            TestProperty(obj, propertyName, startValue, !startValue, true);
        }

        /// <summary>
        /// Asserts that a bool property of an object has the given start value. Sets the property to the toggled start value and asserts that
        /// the toggled value can then be read back from the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="startValue"></param>
        public static void TestProperty<T>(T obj, Expression<Func<T, object>> propertyExpression, bool startValue)
        {
            TestProperty(obj, PropertyHelper.ExtractName(propertyExpression), startValue, !startValue, true);
        }
        #endregion

        #region AssignPropertyValue
        /// <summary>
        /// Assigns a constant dummy value to a property on an object.
        /// </summary>
        /// <param name="obj">The object to assign to.</param>
        /// <param name="property">The property to assign.</param>
        /// <param name="useValue1">True if the first constant value should be assigned, false if the second (different) constant value should be assigned.</param>
        /// <param name="generateValue">Optional func that generates one of two different values for any non-standard type. Expected to throw an exception if the type is unrecognised.</param>
        public static void AssignPropertyValue(object obj, PropertyInfo property, bool useValue1, Func<Type, bool, object> generateValue = null)
        {
            var type = property.PropertyType;
            if(type == typeof(string))          property.SetValue(obj, useValue1 ? "VALUE1" : "VALUE2", null);

            else if(type == typeof(byte))       property.SetValue(obj, useValue1 ? (byte)1 : (byte)2, null);
            else if(type == typeof(short))      property.SetValue(obj, useValue1 ? (short)1 : (short)2, null);
            else if(type == typeof(int))        property.SetValue(obj, useValue1 ? 1 : 2, null);
            else if(type == typeof(long))       property.SetValue(obj, useValue1 ? 1L : 2L, null);
            else if(type == typeof(float))      property.SetValue(obj, useValue1 ? 1F : 2F, null);
            else if(type == typeof(double))     property.SetValue(obj, useValue1 ? 1.0 : 2.0, null);
            else if(type == typeof(bool))       property.SetValue(obj, useValue1 ? true : false, null);
            else if(type == typeof(DateTime))   property.SetValue(obj, useValue1 ? new DateTime(2014, 1, 25) : new DateTime(1920, 8, 17), null);

            else if(type == typeof(byte?))      property.SetValue(obj, useValue1 ? (byte?)null : (byte)2, null);
            else if(type == typeof(short?))     property.SetValue(obj, useValue1 ? (short?)null : (short)2, null);
            else if(type == typeof(int?))       property.SetValue(obj, useValue1 ? (int?)null : 2, null);
            else if(type == typeof(long?))      property.SetValue(obj, useValue1 ? (long?)null : 2L, null);
            else if(type == typeof(bool?))      property.SetValue(obj, useValue1 ? (bool?)null : false, null);
            else if(type == typeof(float?))     property.SetValue(obj, useValue1 ? (float?)null : 2F, null);
            else if(type == typeof(double?))    property.SetValue(obj, useValue1 ? (double?)null : 2.0, null);
            else if(type == typeof(DateTime?))  property.SetValue(obj, useValue1 ? (DateTime?)null : new DateTime(1920, 8, 17), null);

            else {
                if(generateValue == null) {
                    throw new NotImplementedException($"Need to add support for property type {type.Name}");
                } else {
                    property.SetValue(obj, generateValue(type, useValue1));
                }
            }
        }
        #endregion

        #region TestSimpleEquals, TestSimpleGetHashCode, TestObjectPropertiesAreEqual
        /// <summary>
        /// Tests that an object whose Equals method tests all of its public properties is behaving correctly. Only
        /// works with objects that have a simple constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedEquals"></param>
        /// <param name="generateValue">Optional func that generates one of two different values for any non-standard type. Expected to throw an exception if the type is unrecognised.</param>
        public static void TestSimpleEquals(Type type, bool expectedEquals, Func<Type, bool, object> generateValue = null)
        {
            foreach(var property in type.GetProperties().Where(r => r.CanRead && r.CanWrite)) {
                var instance1 = Activator.CreateInstance(type);
                var instance2 = Activator.CreateInstance(type);

                AssignPropertyValue(instance1, property, true, generateValue);
                AssignPropertyValue(instance2, property, expectedEquals, generateValue);

                Assert.AreEqual(expectedEquals, instance1.Equals(instance2), "Property that failed: {0}", property.Name);
            }
        }

        /// <summary>
        /// Tests that an object whose GetHashCode method tests all of its public properties is behaving correctly. Only
        /// works with objects that have a simple constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expectedEquals"></param>
        /// <param name="generateValue">Optional func that generates one of two different values for any non-standard type. Expected to throw an exception if the type is unrecognised.</param>
        /// <remarks>
        /// This only checks that two objects that would pass equality for <see cref="TestSimpleEquals"/> have the same hash code.
        /// </remarks>
        public static void TestSimpleGetHashCode(Type type, Func<Type, bool, object> generateValue = null)
        {
            var instance1 = Activator.CreateInstance(type);
            var instance2 = Activator.CreateInstance(type);

            foreach(var property in type.GetProperties().Where(r => r.CanRead && r.CanWrite)) {
                AssignPropertyValue(instance1, property, true, generateValue);
                AssignPropertyValue(instance2, property, true, generateValue);
            }

            Assert.AreEqual(instance1.GetHashCode(), instance2.GetHashCode());
        }

        /// <summary>
        /// Compares properties on two objects for equality.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="ignoreProperty">Return true if a given property should be ignored.</param>
        /// <param name="testProperty">Return true / false if the two values are equal or null if the default equality test should be applied.</param>
        public static void TestObjectPropertiesAreEqual<T>(T expected, T actual, Func<string, bool> ignoreProperty = null, Func<string, Type, object, object, bool?> testProperty = null)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            if(ignoreProperty == null) {
                ignoreProperty = (propertyName) => false;
            }
            bool? defaultTestPropertyFunc(string propertyName, Type propertyType, object expectedValue, object actualValue) {
                    return Object.Equals(expectedValue, actualValue);
            };
            if(testProperty == null) {
                testProperty = defaultTestPropertyFunc;
            }

            if(!Object.ReferenceEquals(expected, actual)) {
                var objType = typeof(T);
                foreach(var propertyInfo in objType.GetProperties()) {
                    var expectedValue = propertyInfo.GetValue(expected, null);
                    var actualValue = propertyInfo.GetValue(actual, null);

                    if(!ignoreProperty(propertyInfo.Name)) {
                        var areEqual = testProperty(
                            propertyInfo.Name,
                            propertyInfo.PropertyType,
                            expectedValue,
                            actualValue
                        );
                        if(areEqual == null) {
                            areEqual = defaultTestPropertyFunc(
                                propertyInfo.Name,
                                propertyInfo.PropertyType,
                                expectedValue,
                                actualValue
                            );
                        }
                        Assert.IsTrue(areEqual.GetValueOrDefault(), $"Difference found in {propertyInfo.Name} property - expected {expectedValue}, actual was {actualValue}");
                    }
                }
            }
        }
        #endregion

        #region ChangeType
        /// <summary>
        /// Converts a value from one type to another, as per the standard <see cref="Convert.ChangeType(object, Type)"/>,
        /// except that this version can cope with the conversion of <see cref="Nullable{T}"/> types.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type, IFormatProvider provider)
        {
            // Moved the code to Interface so that I could use it in the program
            return VirtualRadar.Interface.CustomConvert.ChangeType(value, type, provider);
        }
        #endregion

        #region ConvertToBitString, ConvertBitStringToBytes
        /// <summary>
        /// Converts a value to a binary representation of a given length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="countDigits"></param>
        /// <returns>The value, expressed as a sequence of binary digits, within a string of countDigits length.</returns>
        public static string ConvertToBitString(long value, int countDigits)
        {
            var bits = Convert.ToString(value, 2);
            if(bits.Length < countDigits) bits = String.Format("{0}{1}", new String('0', countDigits - bits.Length), bits);  // must be a better way to do this :)
            return bits;
        }

        /// <summary>
        /// Converts a value to a binary representation of a given length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="countDigits"></param>
        /// <returns>The value, expressed as a sequence of binary digits, within a string of countDigits length.</returns>
        public static string ConvertToBitString(int value, int countDigits)
        {
            return ConvertToBitString((long)value, countDigits);
        }

        /// <summary>
        /// Converts a string of binary digits (with optional whitespace) to a collection of bytes.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static List<byte> ConvertBitStringToBytes(string bits)
        {
            bits = bits.Trim().Replace(" ", "");
            if(bits.Length % 8 != 0) Assert.Fail("The number of bits must be divisible by 8");

            var result = new List<byte>();
            for(int i = 0;i < bits.Length;i += 8) {
                result.Add(Convert.ToByte(bits.Substring(i, 8), 2));
            }

            return result;
        }
        #endregion

        #region SequenceEqual
        /// <summary>
        /// Returns true if both sequences are null or if the LINQ SequenceEqual method returns true for them, otherwise returns false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool SequenceEqual<T>(IEnumerable<T> lhs, IEnumerable<T> rhs)
        {
            return (lhs == null && rhs == null) || (lhs != null && rhs != null && lhs.SequenceEqual(rhs));
        }
        #endregion

        #region Moq helpers
        /// <summary>
        /// Creates a Moq mock object for a type that implements <see cref="ISingleton{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Mock<T> CreateMockSingleton<T>()
            where T: class
        {
            #pragma warning disable 0618
            if(typeof(ISingleton<T>).IsAssignableFrom(typeof(T))) {
                return CreateMockSingletonInterface<T>();
            } else if(typeof(T).GetCustomAttribute<SingletonAttribute>() != null) {
                return CreateMockSingletonAttribute<T>();
            } else {
                throw new InvalidOperationException($"{nameof(T)} neither implements ISingleton<> or is tagged as a Singleton");
            }
            #pragma warning restore
        }

        private static Mock<T> CreateMockSingletonInterface<T>()
            where T: class
        {
            #pragma warning disable 0618
            if(!typeof(ISingleton<T>).IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException($"{typeof(T).Name} does not implement {typeof(ISingleton<>).Name}");
            }

            var result = new Mock<T>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var strict = new Mock<T>(MockBehavior.Strict);

            var parameter = Expression.Parameter(typeof(ISingleton<T>));
            var body = Expression.Property(parameter, "Singleton");
            var lambda = Expression.Lambda<Func<T, T>>(body, parameter);
            strict.Setup(lambda).Returns(result.Object);

            Factory.RegisterInstance(typeof(T), strict.Object);
            #pragma warning restore

            return result;
        }

        private static Mock<T> CreateMockSingletonAttribute<T>()
            where T: class
        {
            var result = CreateMockInstance<T>();
            Factory.RegisterInstance<T>(result.Object);

            return result;
        }

        /// <summary>
        /// Creates a Moq stub for an object and registers it with the class factory. The instance returned by the method
        /// is the one the code under test will always receive when it asks for an instantiation of the interface.
        /// </summary>
        /// <returns></returns>
        public static Mock<T> CreateMockImplementation<T>()
            where T: class
        {
            #pragma warning disable 0618
            if(typeof(ISingleton<T>).IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException($"{typeof(T).Name} implements {typeof(ISingleton<>).Name}, use CreateMockSingleton instead");
            }

            var result = CreateMockInstance<T>();
            Factory.RegisterInstance(typeof(T), result.Object);
            #pragma warning restore

            return result;
        }

        /// <summary>
        /// Creates a Moq stub for an object but does not register it with the class factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Mock<T> CreateMockInstance<T>()
            where T: class
        {
            return new Mock<T> { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }
        #endregion

        #region FileSystemHelpers - RetryFileIOAction
        /// <summary>
        /// Retries an action until it either stops giving IO errors or too many errors have occurred.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retries">Numberof times to try, defaults to 20</param>
        /// <param name="millisecondsBetweenAttempts">The number of MS to wait after a failed attempt. Defaults to 250.</param>
        /// <remarks>
        /// On my desktop with SSDs the tests occasionally failed because the file was still in use after
        /// it had been disposed of. This function retries file operations until either they stop giving
        /// exceptions or a counter expires.
        /// </remarks>
        public static void RetryFileIOAction(Action action, int retries = 20, int millisecondsBetweenAttempts = 250)
        {
            for(var i = 0;i < retries;++i) {
                var pause = false;
                var giveUp = i + 1 == retries;
                try {
                    action();
                    break;
                } catch(IOException) {
                    if(giveUp) {
                        throw;
                    }
                    pause = true;
                } catch(UnauthorizedAccessException) {
                    if(giveUp) {
                        throw;
                    }
                    pause = true;
                }
                if(pause) {
                    Thread.Sleep(millisecondsBetweenAttempts);
                }
            }
        }
        #endregion
    }
}
