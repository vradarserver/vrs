// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="INetworkFeed"/>.
    /// </summary>
    class NetworkFeed : INetworkFeed
    {
        /// <summary>
        /// True if the feed has been initialised.
        /// </summary>
        protected bool _Initialised;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int UniqueId { get; protected set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftList AircraftList { get; private set; }

        /// <summary>
        /// Returns <see cref="AircraftList"/> cast to an <see cref="IPolarPlottingAircraftList"/>.
        /// </summary>
        protected IPolarPlottingAircraftList PolarPlottingAircraftList => AircraftList as IPolarPlottingAircraftList;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IListener Listener { get; protected set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsVisible { get; private set; }


        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectionStatus ConnectionStatus => Listener?.ConnectionStatus ?? default;

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~NetworkFeed()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs args)
        {
            EventHelper.Raise(ConnectionStateChanged, this, args);
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
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(AircraftList != null) {
                    var polarPlotter = PolarPlottingAircraftList?.PolarPlotter;
                    if(polarPlotter != null) {
                        PolarPlottingAircraftList.PolarPlotter = null;
                        polarPlotter.Dispose();
                    }

                    AircraftList.ExceptionCaught -= AircraftList_ExceptionCaught;
                    AircraftList.Dispose();
                    AircraftList = null;
                }

                if(Listener != null) {
                    Listener.ConnectionStateChanged -= Listener_ConnectionStateChanged;
                    Listener.ExceptionCaught -= Listener_ExceptionCaught;
                    Listener.Dispose();
                    Listener = null;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            if(Listener != null) {
                Listener.Connect();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            if(Listener != null) {
                Listener.Disconnect();
            }
        }

        /// <summary>
        /// Performs initialisation common to all Initialise methods.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="name"></param>
        /// <param name="isSatcomFeed"></param>
        /// <param name="receiverUsage"></param>
        /// <param name="startAircraftList"></param>
        protected void DoCommonInitialise(int uniqueId, string name, bool isSatcomFeed, ReceiverUsage receiverUsage, bool startAircraftList)
        {
            _Initialised = true;
            UniqueId = uniqueId;
            Name = name;

            Listener.ReceiverId = uniqueId;
            Listener.ReceiverName = Name;
            Listener.IsSatcomFeed = isSatcomFeed;

            var baseStationAircraftList = Factory.Resolve<IBaseStationAircraftList>();
            AircraftList = baseStationAircraftList;
            baseStationAircraftList.ExceptionCaught += AircraftList_ExceptionCaught;
            baseStationAircraftList.Listener = Listener;
            baseStationAircraftList.StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();

            SetIsVisible(receiverUsage);

            if(startAircraftList) {
                AircraftList.Start();
            }
        }

        /// <summary>
        /// Sets the <see cref="IsVisible"/> property from the receiver usage passed across.
        /// </summary>
        /// <param name="receiverUsage"></param>
        protected void SetIsVisible(ReceiverUsage receiverUsage)
        {
            IsVisible = receiverUsage == ReceiverUsage.Normal;
        }

        /// <summary>
        /// Saves the polar plot for the feed.
        /// </summary>
        protected void SaveCurrentPolarPlot()
        {
            try {
                var storage = Factory.ResolveSingleton<ISavedPolarPlotStorage>();
                storage.Save(this);
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception while saving polar plot from feed initialisation: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Loads the polar plot for the feed.
        /// </summary>
        private void LoadCurrentPolarPlot()
        {
            try {
                var plottingAircraftList = PolarPlottingAircraftList;
                if(plottingAircraftList != null) {
                    var storage = Factory.ResolveSingleton<ISavedPolarPlotStorage>();
                    var savedPolarPlot = storage.Load(this);

                    if(savedPolarPlot != null && savedPolarPlot.IsForSameFeed(this)) {
                        plottingAircraftList.PolarPlotter.LoadFrom(savedPolarPlot);
                    }
                }
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception while loading polar plot from feed initialisation: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Configures the polar plotter.
        /// </summary>
        /// <param name="receiverLocation"></param>
        /// <param name="nameChanged"></param>
        protected void ConfigurePolarPlotter(ReceiverLocation receiverLocation, bool nameChanged)
        {
            var plottingAircraftList = PolarPlottingAircraftList;
            if(plottingAircraftList != null) {
                if(receiverLocation == null) {
                    plottingAircraftList.PolarPlotter = null;
                } else {
                    var existingPlotter = plottingAircraftList.PolarPlotter;
                    if(existingPlotter == null || existingPlotter.Latitude != receiverLocation.Latitude || existingPlotter.Longitude != receiverLocation.Longitude) {
                        var polarPlotter = existingPlotter ?? Factory.Resolve<IPolarPlotter>();
                        polarPlotter.Initialise(receiverLocation.Latitude, receiverLocation.Longitude);
                        if(existingPlotter == null) {
                            plottingAircraftList.PolarPlotter = polarPlotter;
                            LoadCurrentPolarPlot();
                        }
                    }

                    if(nameChanged && existingPlotter != null) {
                        LoadCurrentPolarPlot();
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the listener catches an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void Listener_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }

        /// <summary>
        /// Raised when the listener changes connection state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void Listener_ConnectionStateChanged(object sender, EventArgs args)
        {
            OnConnectionStateChanged(args);
        }

        /// <summary>
        /// Raised when the aircraft list catches an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void AircraftList_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }
    }
}
