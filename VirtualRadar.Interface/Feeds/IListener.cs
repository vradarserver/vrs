// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.ModeS;

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// The interface for objects that can listen to a stream of bytes and decode them into messages.
    /// </summary>
    public interface IListener : IDisposable
    {
        /// <summary>
        /// Gets or sets the identity of the receiver that is controlling this listener.
        /// </summary>
        int ReceiverId { get; set; }

        /// <summary>
        /// Gets or sets the name of the receiver that is controlling this listener.
        /// </summary>
        string ReceiverName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that this is a satcom feed.
        /// </summary>
        bool IsSatcomFeed { get; set; }

        /// <summary>
        /// Gets the statistics for the listener.
        /// </summary>
        ReceiverStatistics Statistics { get; }

        /// <summary>
        /// Gets the connector that is handling the connection for the listener. Do not modify any properties on the connection directly,
        /// always use <see cref="ChangeSource"/> to perform configuration changes.
        /// </summary>
        IConnector Connector { get; }

        /// <summary>
        /// Gets the object that can extract the important bytes from the stream. Do not modify any properties on the extractor directly,
        /// always use <see cref="ChangeSource"/> to perform configuration changes.
        /// </summary>
        IMessageBytesExtractor BytesExtractor { get; }

        /// <summary>
        /// Gets the object that translates raw messages into Port30003 format messages. Do not modify any properties on the translator directly,
        /// always use <see cref="ChangeSource"/> to perform configuration changes.
        /// </summary>
        IRawMessageTranslator RawMessageTranslator { get; }

        /// <summary>
        /// Gets a value indicating whether the listener is connected to the source of aircraft message data.
        /// </summary>
        ConnectionStatus ConnectionStatus { get; }

        /// <summary>
        /// Gets a count of the total number of messages received by the object.
        /// </summary>
        long TotalMessages { get; }

        /// <summary>
        /// Gets a count of the total number of messages that could not be translated.
        /// </summary>
        /// <remarks>
        /// This is incremented regardless of the value of <see cref="IgnoreBadMessages"/>.
        /// </remarks>
        long TotalBadMessages { get; }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions thrown by the message translator cause a disconnection
        /// and raise ExceptionCaught or whether they are ignored.
        /// </summary>
        bool IgnoreBadMessages { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates that the listener should treat ICAOs on DF18 CF1 messages as valid.
        /// </summary>
        bool AssumeDF18CF1IsIcao { get; set; }

        /// <summary>
        /// Raised when bytes have been received from the data source. This may not be on the same thread that started
        /// the listener - however all messages are guaranteed to be transmitted in the order in which they were received.
        /// </summary>
        event EventHandler<EventArgs<byte[]>> RawBytesReceived;

        /// <summary>
        /// Raised when bytes representing a Mode-S message have been received. This may not be on the same thread that started
        /// the listener - however all messages are guaranteed to be transmitted in the order in which they were received.
        /// </summary>
        /// <remarks>
        /// This is only raised when listening to sources of Mode-S data. It is not raised when listening to Port 30003 format
        /// sources. The ExtractedBytes passed as the event argument is a clone of the bytes original extracted. Bytes that fail
        /// the checksum test (when applicable) are not passed to the event handler.
        /// </remarks>
        event EventHandler<EventArgs<ExtractedBytes>> ModeSBytesReceived;

        /// <summary>
        /// Raised when a message has been received from a source of Port30003 data. This may not be on the same thread
        /// that started the listener - however all messages are guaranteed to be transmitted in the order in which they
        /// were received.
        /// </summary>
        /// <remarks>
        /// This is raised when listening to both Mode-S and Port 30003 data feeds. When listening to Mode-S sources the
        /// listener uses an <see cref="IRawMessageTranslator"/> to create the Port 30003 message.
        /// </remarks>
        event EventHandler<BaseStationMessageEventArgs> Port30003MessageReceived;

        /// <summary>
        /// Raised when a message has been received from the source of raw data. This may not be on the same thread that
        /// started the listener - however all messages are guaranteed to be transmitted in the order in which they
        /// were received.
        /// </summary>
        /// <remarks>
        /// This is only raised when listening to sources of Mode-S data. It is not raised when listening to Port 30003 format
        /// sources.
        /// </remarks>
        event EventHandler<ModeSMessageEventArgs> ModeSMessageReceived;

        /// <summary>
        /// Raised when the listener connects or disconnects. Note that exceptions raised during parsing of
        /// messages will cause the object to automatically disconnect.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raised when the listener is asked to change source of data. If the change in source causes a disconnect and
        /// reconnect then this event is raised after the disconnect but before the reconnect.
        /// </summary>
        event EventHandler SourceChanged;

        /// <summary>
        /// Raised when a decoder that the listener is using to translate messages indicates that the previous position
        /// reported for an aircraft was wrong and that its position has been reset.
        /// </summary>
        /// <remarks>
        /// The event args carries the ICAO24 code for the aircraft whose position has been reset. Any tracks held for
        /// the aircraft should be cleared down and restarted. This message is only used in conjunction with raw
        /// message decoding.
        /// </remarks>
        event EventHandler<EventArgs<string>> PositionReset;

        /// <summary>
        /// Changes the connector and/or message bytes extractor used by the listener.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="bytesExtractor"></param>
        /// <param name="rawMessageTranslator"></param>
        void ChangeSource(IConnector connector, IMessageBytesExtractor bytesExtractor, IRawMessageTranslator rawMessageTranslator);

        /// <summary>
        /// Connects to the source of aircraft data. Incoming messages from the source will raise events on the listener.
        /// </summary>
        /// <remarks>
        /// The method begins the connection procedure on a background thread and returns almost immediately. It may not have connected by the time the
        /// connection returns. It may raise an exception if <see cref="ChangeSource"/> has not been called but otherwise any exceptions raised during the
        /// connection procedure are passed through ExceptionCaught.
        /// </remarks>
        void Connect();

        /// <summary>
        /// Called implicitly by Dispose, disconnects from the source of aircraft message data.
        /// </summary>
        void Disconnect();
    }
}
