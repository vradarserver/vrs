// Copyright © 2015 onwards, Andrew Whewell
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
    /// The interface for objects that can serialise objects to XML and deserialise objects from XML.
    /// </summary>
    /// <remarks>
    /// The implementation of this interface doesn't need to be able to serialise and deserialise everything
    /// that XmlSerializer can do, it just needs to be able to do enough to handle the Configuration.xml file.
    /// I used to use XmlSerializer to read and write this but then Mono started whining about namespaces, so
    /// I needed to work around Mono (again). This is the result. I'm not writing anything else in XML, any new
    /// serialisation is going to use JSON. However I can't switch the configuration over to JSON, it would make
    /// life hard for people who want to roll back to pre-JSON-configuration versions of VRS.
    /// </remarks>
    public interface IXmlSerialiser
    {
        /// <summary>
        /// Gets or sets a value indicating that attempts to serialise objects that can't be deserialised
        /// by System.Xml.XmlSerializer will throw an exception. Defaults to true.
        /// </summary>
        bool XmlSerializerCompatible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that attempts to deserialise an unrecognised enum value will
        /// deserialise to the default enum value rather than throw an exception. Defaults to false.
        /// </summary>
        bool UseDefaultEnumValueIfUnknown { get; set; }

        /// <summary>
        /// Serialises the object to a stream.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        void Serialise(object obj, Stream stream);

        /// <summary>
        /// Serialises the object to a text writer.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="textWriter"></param>
        void Serialise(object obj, TextWriter textWriter);

        /// <summary>
        /// Deserialises the object from a stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        T Deserialise<T>(Stream stream)
            where T: class, new();

        /// <summary>
        /// Deserialises the object from a text writer.
        /// </summary>
        /// <param name="textReader"></param>
        /// <returns></returns>
        T Deserialise<T>(TextReader textReader)
            where T: class, new();
    }
}
