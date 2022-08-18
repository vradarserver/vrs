﻿// Copyright © 2022 onwards, Andrew Whewell
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
using System.Threading.Tasks;

namespace VirtualRadar.Plugin.Vatsim
{
    class Options : INotifyPropertyChanged
    {
        private bool _Enabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is downloading VATSIM states and building
        /// feeds from them.
        /// </summary>
        public bool Enabled
        {
            get => _Enabled;
            set => SetField(ref _Enabled, value, nameof(Enabled));
        }

        private int _RefreshIntervalSeconds = 15;
        /// <summary>
        /// Gets or sets the number of seconds between fetches of VATSIM data. Note that there is a rate limit
        /// at VATSIM of 15 seconds (as of time of writing).
        /// </summary>
        public int RefreshIntervalSeconds
        {
            get => _RefreshIntervalSeconds;
            set => SetField(ref _RefreshIntervalSeconds, value, nameof(RefreshIntervalSeconds));
        }

        private bool _AssumeSlowAircraftAreOnGround = true;
        /// <summary>
        /// Gets or sets a value indicating that on-ground status is to be inferred from the speed of the
        /// aircraft. See <see cref="SlowAircraftThresholdSpeed"/>.
        /// </summary>
        public bool AssumeSlowAircraftAreOnGround
        {
            get => _AssumeSlowAircraftAreOnGround;
            set => SetField(ref _AssumeSlowAircraftAreOnGround, value, nameof(AssumeSlowAircraftAreOnGround));
        }

        private int _SlowAircraftThresholdSpeedKnots = 40;
        /// <summary>
        /// Gets or sets an aircraft speed (in knots) below which aircraft are considered to be on the ground if
        /// <see cref="AssumeSlowAircraftAreOnGround"/> is true.
        /// </summary>
        public int SlowAircraftThresholdSpeed
        {
            get => _SlowAircraftThresholdSpeedKnots;
            set => SetField(ref _SlowAircraftThresholdSpeedKnots, value, nameof(SlowAircraftThresholdSpeed));
        }

        private bool _InferModelFromModelType = true;
        /// <summary>
        /// Gets or sets a value indicating that the manufacturer and model should be inferred from the reported
        /// model type.
        /// </summary>
        public bool InferModelFromModelType
        {
            get => _InferModelFromModelType;
            set => SetField(ref _InferModelFromModelType, value, nameof(InferModelFromModelType));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

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
    }
}
