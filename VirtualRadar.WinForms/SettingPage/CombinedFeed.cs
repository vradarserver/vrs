// Copyright © 2014 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Describes a feed item, which is derived from either a receiver or a merged feed.
    /// </summary>
    public class CombinedFeed : INotifyPropertyChanged, IDisposable
    {
        private static string _UniqueIdName;
        private static string _NameName;
        private static string _ReceiverUniqueIdName;
        private static string _ReceiverNameName;
        private static string _MergedFeedUniqueIdName;
        private static string _MergedFeedNameName;

        /// <summary>
        /// Initialises the static fields.
        /// </summary>
        static CombinedFeed()
        {
            _UniqueIdName =             PropertyHelper.ExtractName<CombinedFeed>(r => r.UniqueId);
            _NameName =                 PropertyHelper.ExtractName<CombinedFeed>(r => r.Name);
            _ReceiverUniqueIdName =     PropertyHelper.ExtractName<Receiver>(r => r.UniqueId);
            _ReceiverNameName =         PropertyHelper.ExtractName<Receiver>(r => r.Name);
            _MergedFeedUniqueIdName =   PropertyHelper.ExtractName<MergedFeed>(r => r.UniqueId);
            _MergedFeedNameName =       PropertyHelper.ExtractName<MergedFeed>(r => r.Name);
        }

        /// <summary>
        /// Gets the receiver that is a part of the combined feed.
        /// </summary>
        public Receiver Receiver { get; private set; }

        /// <summary>
        /// Gets the merged feed that is a part of the combined feed.
        /// </summary>
        public MergedFeed MergedFeed { get; private set; }

        /// <summary>
        /// Gets the unique ID of the receiver or merged feed.
        /// </summary>
        public int UniqueId { get { return Receiver != null ? Receiver.UniqueId : MergedFeed != null ? MergedFeed.UniqueId : 0; } }

        /// <summary>
        /// Gets the name of the receiver or merged feed.
        /// </summary>
        public string Name { get { return Receiver != null ? Receiver.Name : MergedFeed != null ? MergedFeed.Name : null; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="receiver"></param>
        public CombinedFeed(Receiver receiver)
        {
            Receiver = receiver;
            Receiver.PropertyChanged += Wrapped_PropertyChanged;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="mergedFeed"></param>
        public CombinedFeed(MergedFeed mergedFeed)
        {
            MergedFeed = mergedFeed;
            MergedFeed.PropertyChanged += Wrapped_PropertyChanged;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CombinedFeed()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes or finalises an object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(Receiver != null) Receiver.PropertyChanged -= Wrapped_PropertyChanged;
                else                 MergedFeed.PropertyChanged -= Wrapped_PropertyChanged;
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", UniqueId, Name);
        }

        /// <summary>
        /// Called when a property changed on the wrapped object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Wrapped_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(Receiver != null) {
                if(args.PropertyName == _ReceiverUniqueIdName)      OnPropertyChanged(new PropertyChangedEventArgs(_UniqueIdName));
                else if(args.PropertyName == _ReceiverNameName)     OnPropertyChanged(new PropertyChangedEventArgs(_NameName));
            } else {
                if(args.PropertyName == _MergedFeedUniqueIdName)    OnPropertyChanged(new PropertyChangedEventArgs(_UniqueIdName));
                else if(args.PropertyName == _MergedFeedNameName)   OnPropertyChanged(new PropertyChangedEventArgs(_NameName));
            }
        }
    }
}
