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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Settings that control how browsers on the Internet interact with the server.
    /// </summary>
    public class InternetClientSettings : INotifyPropertyChanged
    {
        private bool _CanRunReports;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to run reports.
        /// </summary>
        public bool CanRunReports
        {
            get { return _CanRunReports; }
            set { SetField(ref _CanRunReports, value, nameof(CanRunReports)); }
        }

        private bool _CanShowPinText;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to show text labels on the aircraft pins.
        /// </summary>
        public bool CanShowPinText
        {
            get { return _CanShowPinText; }
            set { SetField(ref _CanShowPinText, value, nameof(CanShowPinText)); }
        }

        private bool _CanPlayAudio;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to play audio from the server.
        /// </summary>
        public bool CanPlayAudio
        {
            get { return _CanPlayAudio; }
            set { SetField(ref _CanPlayAudio, value, nameof(CanPlayAudio)); }
        }

        private bool _CanShowPictures;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients are allowed to see aircraft pictures.
        /// </summary>
        public bool CanShowPictures
        {
            get { return _CanShowPictures; }
            set { SetField(ref _CanShowPictures, value, nameof(CanShowPictures)); }
        }

        private int _TimeoutMinutes;
        /// <summary>
        /// Gets or sets the number of minutes of inactivity before the client times out and stops asking for the aircraft list.
        /// If this is zero then the timeout is disabled.
        /// </summary>
        public int TimeoutMinutes
        {
            get { return _TimeoutMinutes; }
            set { SetField(ref _TimeoutMinutes, value, nameof(TimeoutMinutes)); }
        }

        private bool _AllowInternetProximityGadgets;
        /// <summary>
        /// Gets or sets a value indicating whether proximity gadgets can connect to this server over the Internet.
        /// </summary>
        public bool AllowInternetProximityGadgets
        {
            get { return _AllowInternetProximityGadgets; }
            set { SetField(ref _AllowInternetProximityGadgets, value, nameof(AllowInternetProximityGadgets)); }
        }

        private bool _CanSubmitRoutes;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients can see the links to submit routes.
        /// </summary>
        public bool CanSubmitRoutes
        {
            get { return _CanSubmitRoutes; }
            set { SetField(ref _CanSubmitRoutes, value, nameof(CanSubmitRoutes)); }
        }

        private bool _CanShowPolarPlots;
        /// <summary>
        /// Gets or sets a value indicating that Internet clients can see polar plots.
        /// </summary>
        public bool CanShowPolarPlots
        {
            get { return _CanShowPolarPlots; }
            set { SetField(ref _CanShowPolarPlots, value, nameof(CanShowPolarPlots)); }
        }

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
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public InternetClientSettings()
        {
            TimeoutMinutes = 20;
        }
    }
}
