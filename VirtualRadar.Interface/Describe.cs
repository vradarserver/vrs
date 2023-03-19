﻿// Copyright © 2010 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Localisation;
using System.IO.Ports;
using System.Net.Sockets;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A static class that can produce descriptions of objects that implement common interfaces.
    /// </summary>
    public static class Describe
    {
        /// <summary>
        /// Returns a standard description of an airport.
        /// </summary>
        /// <param name="airport"></param>
        /// <param name="preferIataAirportCode"></param>
        /// <param name="showCode"></param>
        /// <param name="showName"></param>
        /// <param name="showCountry"></param>
        /// <returns></returns>
        public static string Airport(Airport airport, bool preferIataAirportCode, bool showCode = true, bool showName = true, bool showCountry = true)
        {
            var result = new StringBuilder();

            if(airport != null) {
                if(showCode) {
                    var firstChoice = preferIataAirportCode ? airport.IataCode : airport.IcaoCode;
                    var secondChoice = preferIataAirportCode ? airport.IcaoCode : airport.IataCode;
                    if(!String.IsNullOrEmpty(firstChoice)) {
                        result.Append(firstChoice);
                    } else if(!String.IsNullOrEmpty(secondChoice)) {
                        result.Append(secondChoice);
                    }
                }
                if(showName && !String.IsNullOrEmpty(airport.Name)) {
                    AddSeparator(result, " ").Append(airport.Name);
                }
                if(showCountry && !String.IsNullOrEmpty(airport.Country)) {
                    AddSeparator(result, ", ").Append(airport.Country);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a single airport name that is a concatenation of the official airport name and location / city name.
        /// </summary>
        /// <param name="airportName"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public static string AirportName(string airportName, string cityName)
        {
            var result = new StringBuilder(airportName ?? "");
            if(!String.IsNullOrEmpty(cityName)) {
                if(result.Length == 0
                   || (
                          airportName != cityName
                       && !airportName.StartsWith($"{cityName} ")
                       && airportName.IndexOf($" {cityName}") == -1
                   )
                ) {
                    AddSeparator(result, ", ").Append(cityName);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a number of bytes formatted as 'n B', 'n KB', 'n MB' etc.
        /// </summary>
        /// <param name="fullSize"></param>
        /// <returns></returns>
        public static string Bytes(long fullSize)
        {
            var result = "";

            double size = fullSize;
            if(fullSize < 1024) {
                result = $"{size:N0} {Strings.AcronymByte}";
            } else if(fullSize < 0x100000L) {
                result = $"{size / 1024.0:N2} {Strings.AcronymKilobyte}";
            } else if(fullSize < 0x40000000L) {
                result = $"{size / 1048576.0:N2} {Strings.AcronymMegabyte}";
            } else if(fullSize < 0x10000000000L) {
                result = $"{size / 1073741824.0:N2} {Strings.AcronymGigabyte}";
            } else {
                result = $"{size / 1099511627776.0:N2} {Strings.AcronymTerrabyte}";
            }

            return result;
        }

        /// <summary>
        /// Returns the default access enum formatted as a translated string.
        /// </summary>
        /// <param name="defaultAccess"></param>
        /// <returns></returns>
        public static string DefaultAccess(DefaultAccess defaultAccess)
        {
            switch(defaultAccess) {
                case Settings.DefaultAccess.Allow:          return Strings.AllowAccess;
                case Settings.DefaultAccess.Deny:           return Strings.DenyAccess;
                case Settings.DefaultAccess.Unrestricted:   return Strings.UnrestrictedAccess;
                default:                                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the timespan formatted as a number of hours, minutes and seconds.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string TimeSpan(TimeSpan timeSpan) => $"{(long)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";

        /// <summary>
        /// Appends the separator to the string if the string isn't empty. Returns the string so that you can chain it.
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private static StringBuilder AddSeparator(StringBuilder stringBuilder, string separator)
        {
            if(stringBuilder.Length > 0) {
                stringBuilder.Append(separator);
            }
            return stringBuilder;
        }

        /// <summary>
        /// Returns the registration passed across with characters that contravene the ICAO registration rules stripped out or
        /// transformed to upper-case as appropriate. Plus and minus are also accepted.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string IcaoCompliantRegistration(string registration)
        {
            string result = null;

            if(registration != null) {
                var buffer = new StringBuilder();
                foreach(var ch in registration) {
                    if(   (ch >= '0' && ch <= '9')
                       || (ch >= 'a' && ch <= 'z')
                       || (ch >= 'A' && ch <= 'Z')
                       || ch == '-'
                       || ch == '+'
                    ) {
                        buffer.Append(Char.ToUpper(ch));
                    }
                }

                result = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// Returns a description of the collection of <see cref="RebroadcastSettings"/> passed across.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string RebroadcastSettingsCollection(IList<RebroadcastSettings> settings)
        {
            var result = "";

            if(settings == null || settings.Count == 0) {
                result = Strings.RebroadcastServersNoneConfigured;
            } else {
                var enabledCount = settings.Count(r => r.Enabled);
                if(settings.Count == 1) {
                    result = String.Format(Strings.RebroadcastServersDescribeSingle, enabledCount);
                } else {
                    result = String.Format(Strings.RebroadcastServersDescribeMany, settings.Count, enabledCount);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a description of a connection type.
        /// </summary>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        public static string ConnectionType(ConnectionType connectionType)
        {
            switch(connectionType) {
                case Settings.ConnectionType.COM:   return Strings.USBOverCOM;
                case Settings.ConnectionType.TCP:   return Strings.Network;
                case Settings.ConnectionType.HTTP:  return Strings.Http;
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a connection status.
        /// </summary>
        /// <param name="connectionStatus"></param>
        /// <returns></returns>
        public static string ConnectionStatus(Feeds.ConnectionStatus connectionStatus)
        {
            switch(connectionStatus) {
                case Feeds.ConnectionStatus.CannotConnect:  return Strings.CannotConnect;
                case Feeds.ConnectionStatus.Connecting:     return Strings.Connecting;
                case Feeds.ConnectionStatus.Connected:      return Strings.Connected;
                case Feeds.ConnectionStatus.Disconnected:   return Strings.Disconnected;
                case Feeds.ConnectionStatus.Reconnecting:   return Strings.Reconnecting;
                case Feeds.ConnectionStatus.Waiting:        return Strings.Waiting;
                default:                                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a number of stop bits.
        /// </summary>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public static string StopBits(StopBits stopBits)
        {
            switch(stopBits) {
                case System.IO.Ports.StopBits.None:         return Strings.SerialStopBitsNone;
                case System.IO.Ports.StopBits.One:          return Strings.SerialStopBitsOne;
                case System.IO.Ports.StopBits.OnePointFive: return Strings.SerialStopBitsOnePointFive;
                case System.IO.Ports.StopBits.Two:          return Strings.SerialStopBitsTwo;
                default:                                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a parity value.
        /// </summary>
        /// <param name="parity"></param>
        /// <returns></returns>
        public static string Parity(Parity parity)
        {
            switch(parity) {
                case System.IO.Ports.Parity.Even:   return Strings.SerialParityEven;
                case System.IO.Ports.Parity.Mark:   return Strings.SerialParityMark;
                case System.IO.Ports.Parity.None:   return Strings.SerialParityNone;
                case System.IO.Ports.Parity.Odd:    return Strings.SerialParityOdd;
                case System.IO.Ports.Parity.Space:  return Strings.SerialParitySpace;
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a serial port handshake.
        /// </summary>
        /// <param name="handshake"></param>
        /// <returns></returns>
        public static string Handshake(Handshake handshake)
        {
            switch(handshake) {
                case System.IO.Ports.Handshake.None:                    return Strings.SerialHandshakeNone;
                case System.IO.Ports.Handshake.RequestToSend:           return Strings.SerialHandshakeRts;
                case System.IO.Ports.Handshake.RequestToSendXOnXOff:    return Strings.SerialHandshakeRtsXOnXOff;
                case System.IO.Ports.Handshake.XOnXOff:                 return Strings.SerialHandshakeXOnXOff;
                default:                                                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a distance unit.
        /// </summary>
        /// <param name="distanceUnit"></param>
        /// <returns></returns>
        public static string DistanceUnit(DistanceUnit distanceUnit)
        {
            switch(distanceUnit) {
                case Interface.DistanceUnit.Kilometres:     return Strings.Kilometres;
                case Interface.DistanceUnit.Miles:          return Strings.Miles;
                case Interface.DistanceUnit.NauticalMiles:  return Strings.NauticalMiles;
                default:                                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a height unit.
        /// </summary>
        /// <param name="heightUnit"></param>
        /// <returns></returns>
        public static string HeightUnit(HeightUnit heightUnit)
        {
            switch(heightUnit) {
                case Interface.HeightUnit.Feet:     return Strings.Feet;
                case Interface.HeightUnit.Metres:   return Strings.Metres;
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a speed unit.
        /// </summary>
        /// <param name="speedUnit"></param>
        /// <returns></returns>
        public static string SpeedUnit(SpeedUnit speedUnit)
        {
            switch(speedUnit) {
                case Interface.SpeedUnit.KilometresPerHour: return Strings.KilometresPerHour;
                case Interface.SpeedUnit.Knots:             return Strings.Knots;
                case Interface.SpeedUnit.MilesPerHour:      return Strings.MilesPerHour;
                default:                                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a proxy type.
        /// </summary>
        /// <param name="proxyType"></param>
        /// <returns></returns>
        public static string ProxyType(ProxyType proxyType)
        {
            switch(proxyType) {
                case Settings.ProxyType.Forward:        return Strings.Forward;
                case Settings.ProxyType.Reverse:        return Strings.Reverse;
                case Settings.ProxyType.Unknown:        return Strings.Unknown;
                default:                                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a description of a connector activity type.
        /// </summary>
        /// <param name="connectorActivityType"></param>
        /// <returns></returns>
        public static string ConnectorActivityType(Feeds.ConnectorActivityType connectorActivityType)
        {
            switch(connectorActivityType) {
                case Feeds.ConnectorActivityType.Connected:     return Strings.Connected;
                case Feeds.ConnectorActivityType.Disconnected:  return Strings.Disconnected;
                case Feeds.ConnectorActivityType.Exception:     return Strings.Exception;
                case Feeds.ConnectorActivityType.Miscellaneous: return Strings.Miscellaneous;
                default:                                        throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a multi-line description of an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public static string ExceptionMultiLine(Exception exception, string newLine = null)
        {
            newLine ??= Environment.NewLine;
            var result = "";

            if(exception != null) {
                var buffer = new StringBuilder();

                for(var ex = exception;ex != null;ex = ex.InnerException) {
                    if(buffer.Length > 0) {
                        buffer.AppendFormat("-- INNER EXCEPTION --{0}", newLine);
                    }
                    buffer.AppendFormat("{0}: {1}{2}", ex.GetType().FullName, ex.Message, newLine);

                    var socketException = ex as SocketException;
                    if(socketException != null) {
                        buffer.AppendFormat("Socket I/O error, error = {0}, native = {1}, code = {2}", socketException.ErrorCode, socketException.NativeErrorCode, socketException.SocketErrorCode);
                    }

                    buffer.AppendFormat("{0}{1}", ex.StackTrace == null ? "No stack trace" : ex.StackTrace.ToString(), newLine);
                }

                result = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// Returns a full (and probably very long) description of an exception on a single line.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string ExceptionSingleLineFull(Exception exception) => ExceptionMultiLine(exception, "; ");

        /// <summary>
        /// Returns a translated description of a receiver usage.
        /// </summary>
        /// <param name="receiverUsage"></param>
        /// <returns></returns>
        public static string ReceiverUsage(ReceiverUsage receiverUsage)
        {
            switch(receiverUsage) {
                case Settings.ReceiverUsage.Normal:             return Strings.Normal;
                case Settings.ReceiverUsage.HideFromWebSite:    return Strings.Hidden;
                case Settings.ReceiverUsage.MergeOnly:          return Strings.MergeOnly;
                default:                                        throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns an ISO-formatted date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string IsoDate(DateTime date) => date.ToString("yyyy-MM-dd");

        /// <summary>
        /// Returns a description of a map provider.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public static string MapProvider(MapProvider mapProvider)
        {
            switch(mapProvider) {
                case Settings.MapProvider.GoogleMaps:   return Strings.GoogleMaps;
                case Settings.MapProvider.Leaflet:      return Strings.Leaflet;
                default:                                throw new NotImplementedException();
            }
        }
    }
}
