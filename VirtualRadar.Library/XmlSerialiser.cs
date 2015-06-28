// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IXmlSerialiser"/>.
    /// </summary>
    class XmlSerialiser : IXmlSerialiser
    {
        #region Fields
        /// <summary>
        /// The namespace that we assign the XSI alias to.
        /// </summary>
        private static readonly string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// The namespace that we assign the XSD alias to.
        /// </summary>
        private static readonly string XsdNamespace = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// The attribute that marks nil values.
        /// </summary>
        private static readonly XName NilAttributeName = XName.Get("nil", XsiNamespace);

        /// <summary>
        /// A map of value type names to the names that XmlSerializer uses when serialising them.
        /// </summary>
        private static readonly Dictionary<string, string> ValueTypeNameMap = new Dictionary<string,string>() {
            { "Byte",   "UnsignedByte" },
            { "Int16",  "Short" },
            { "Int32",  "Int" },
            { "Int64",  "Long" },
            { "Single", "Float" },
        };
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        public bool XmlSerializerCompatible { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public XmlSerialiser()
        {
            XmlSerializerCompatible = true;
        }
        #endregion

        #region Serialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        public void Serialise(object obj, Stream stream)
        {
            if(obj == null) throw new ArgumentNullException("obj");
            if(stream == null) throw new ArgumentNullException("stream");

            var content = Serialise(obj);
            var bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="textWriter"></param>
        public void Serialise(object obj, TextWriter textWriter)
        {
            if(obj == null) throw new ArgumentNullException("obj");
            if(textWriter == null) throw new ArgumentNullException("textWriter");

            var content = Serialise(obj);
            textWriter.Write(content);
        }

        /// <summary>
        /// Handles the serialisation of objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string Serialise(object obj)
        {
            var root = CreateRoot(obj);
            var xdoc = new XDocument(root);

            var result = new StringBuilder();
            using(var stringWriter = new StringWriterWithEncoding(result, Encoding.UTF8)) {
                using(var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true, IndentChars = "  ", })) {
                    xdoc.WriteTo(xmlWriter);
                }
            }

            return result.ToString();
        }

        private XElement CreateRoot(object value)
        {
            var result = CreateValue(value);
            result.SetAttributeValue(XNamespace.Xmlns + "xsi", XsiNamespace);
            result.SetAttributeValue(XNamespace.Xmlns + "xsd", XsdNamespace);

            return result;
        }

        private XElement CreateValue(object value, string name = null, Type listElementType = null)
        {
            XElement result = null;

            var type = listElementType ?? (value == null ? null : value.GetType());
            name = name ?? GetValueName(type, lowerCamelCaseValueTypeName: listElementType != null);
            AssertXmlSerializerCompatibleType(type);

            if(value != null) {
                result = new XElement(name);
                AddChildNodes(result, value, type);
            } else {
                if(listElementType != null) {
                    result = new XElement(name, new XAttribute(NilAttributeName, true));
                }
            }

            return result;
        }

        private void AddChildNodes(XElement element, object obj, Type objType)
        {
            if(objType.IsValueType || typeof(string) == objType) {
                var valueTypeValue = GetXmlSerializerCompatibleValue(obj, objType);
                element.SetValue(valueTypeValue);
            } else if(typeof(IList).IsAssignableFrom(objType)) {
                var elementType = objType.GetGenericArguments().Single();
                AssertXmlSerializerCompatibleType(elementType);
                foreach(var elementValue in (IList)obj) {
                    var elementNode = CreateValue(elementValue, listElementType: elementType);
                    element.Add(elementNode);
                }
            } else {
                foreach(var property in objType.GetProperties()) {
                    var hasIgnoreAttribute = property.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: false).Length > 0;
                    if(!hasIgnoreAttribute) {
                        AssertXmlSerializerCompatibleType(property.PropertyType);
                        var value = property.GetValue(obj, null);
                        if(value != null) {
                            var type = property.PropertyType;
                            var isString = type == typeof(string);
                            var isList = typeof(IList).IsAssignableFrom(type);
                            var isClass = !type.IsValueType && !isList && !isString;

                            value = GetXmlSerializerCompatibleValue(value, type);

                            XElement childNode;
                            if(isClass) {
                                childNode = CreateValue(value, property.Name);
                            } else {
                                childNode = new XElement(property.Name);
                                if(isList) {
                                    AddChildNodes(childNode, value, property.PropertyType);
                                } else {
                                    childNode.SetValue(value);
                                }
                            }
                            element.Add(childNode);
                        }
                    }
                }
            }
        }

        private object GetXmlSerializerCompatibleValue(object value, Type type)
        {
            var result = value;

            if(type == typeof(char)) {
                result = (int)(char)value;                  // XmlSerializer writes chars as ints corresponding to the Unicode char
            } else if(type == typeof(TimeSpan)) {
                result = ((TimeSpan)value).ToString();      // value.ToString() doesn't work, gives type name. Needs to be unboxed first.
            }

            return result;
        }

        private void AssertXmlSerializerCompatibleType(Type type)
        {
            if(XmlSerializerCompatible) {
                if(type == typeof(IntPtr) ||
                   type == typeof(TimeSpan)) {
                    throw new InvalidOperationException(String.Format("Cannot serialise {0} types in XmlSerializer compatible mode", type.Name));
                }
            }
        }
        #endregion

        #region Deserialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T Deserialise<T>(Stream stream)
            where T: class, new()
        {
            if(stream == null) throw new ArgumentNullException("stream");

            using(var streamReader = new StreamReader(stream, Encoding.UTF8)) {
                var xdoc = XDocument.Load(streamReader);
                return Deserialise<T>(xdoc);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="textReader"></param>
        /// <returns></returns>
        public T Deserialise<T>(TextReader textReader)
            where T: class, new()
        {
            if(textReader == null) throw new ArgumentNullException("textReader");
            var xdoc = XDocument.Load(textReader);

            return Deserialise<T>(xdoc);
        }

        /// <summary>
        /// Does the work for the public deserialise entry points.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        private T Deserialise<T>(XDocument xdoc)
            where T: class, new()
        {
            T result = (T)ReadValue(xdoc.Document.Root, typeof(T).Name, typeof(T));

            return result;
        }

        private object ReadValue(XElement node, string name, Type type, bool isListElement = false)
        {
            object result = null;

            name = name ?? GetValueName(type, lowerCamelCaseValueTypeName: isListElement);
            if(name == node.Name) {
                var nilAttribute = node.Attribute(NilAttributeName);
                var isNil = nilAttribute != null && nilAttribute.Value == "true";
                if(!isNil) {
                    var isString = type == typeof(String);
                    var isList = typeof(IList).IsAssignableFrom(type);
                    var isClass = !type.IsValueType && !isString && !isList;

                    if(isClass) {
                        result = Activator.CreateInstance(type);
                        ReadProperties(node, result);
                    } else if(isList) {
                        result = Activator.CreateInstance(type);
                        ReadList(node, (IList)result);
                    } else {
                        var valueText = node.Value;
                        var isChar = type == typeof(char);     // XmlSerializer writes chars as ints... 
                        var convertToType = isChar ? typeof(int) : type;
                        result = CustomConvert.ChangeType(valueText, convertToType, CultureInfo.InvariantCulture);
                        if(isChar) result = (char)(int)result;
                    }
                }
            }

            return result;
        }

        private void ReadProperties(XElement element, object obj)
        {
            foreach(var property in obj.GetType().GetProperties()) {
                var name = property.Name;
                var node = element.Nodes().OfType<XElement>().SingleOrDefault(r => r.Name == name);
                if(node != null) {
                    var isList = typeof(IList).IsAssignableFrom(property.PropertyType);

                    if(isList) {
                        ReadList(node, (IList)property.GetValue(obj, null));
                    } else {
                        object value = ReadValue(node, property.Name, property.PropertyType);
                        property.SetValue(obj, value, null);
                    }
                }
            }
        }

        private void ReadList(XElement listNode, IList list)
        {
            var listType = list.GetType();
            var elementType = listType.GetGenericArguments().Single();
            foreach(var elementNode in listNode.Nodes().OfType<XElement>()) {
                var elementValue = ReadValue(elementNode, null, elementType, isListElement: true);
                list.Add(elementValue);
            }
        }
        #endregion

        #region Utility methods - GetValueName
        /// <summary>
        /// Returns the element name for a type.
        /// </summary>
        /// <param name="lowerCamelCaseValueTypeName"></param>
        /// <returns></returns>
        private string GetValueName(Type type, bool lowerCamelCaseValueTypeName = false)
        {
            var result = type.Name;

            if(type.IsValueType || type == typeof(string)) {
                string mappedName;
                if(ValueTypeNameMap.TryGetValue(result, out mappedName)) {
                    result = mappedName;
                }
                if(lowerCamelCaseValueTypeName && !String.IsNullOrEmpty(result)) {
                    result = String.Format("{0}{1}", Char.ToLower(result[0]), result.Substring(1));
                }
            } else {
                var isList = typeof(IList).IsAssignableFrom(type);
                if(isList) {
                    var elementType = type.GetGenericArguments().Single();
                    result = String.Format("ArrayOf{0}", GetValueName(elementType));
                }
            }

            return result;
        }
        #endregion
    }
}
