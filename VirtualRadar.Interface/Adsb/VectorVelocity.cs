// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// The speed and bearing of a vehicle specified as two velocities along the X and Y axis.
    /// </summary>
    /// <remarks><para>
    /// ADS-B allows velocity and heading to be specified in one of two ways. One is an explicit
    /// declaration of the heading as a bearing and a velocity. The other is declared as two speeds,
    /// one in the east-west direction and the other in the north-south direction, from which the
    /// bearing and actual speed can be calculated using trigonometry. This class declares the parameters
    /// transmitted for the latter and also carries the code to derive the bearing and speed.
    /// </para></remarks>
    public class VectorVelocity
    {
        /// <summary>
        /// True if the velocity and heading have been calculated using the current properties.
        /// </summary>
        private bool _CalculatedValues;

        private bool _IsWesterlyVelocity;
        /// <summary>
        /// Gets or sets a value indicating that the east-west velocity is west.
        /// </summary>
        public bool IsWesterlyVelocity
        {
            get => _IsWesterlyVelocity;
            set {
                if(_IsWesterlyVelocity != value) {
                    _CalculatedValues = false;
                    _IsWesterlyVelocity = value;
                }
            }
        }

        private bool _IsSoutherlyVelocity;
        /// <summary>
        /// Gets or sets a value indicating that the north-south velocity is south.
        /// </summary>
        public bool IsSoutherlyVelocity
        {
            get => _IsSoutherlyVelocity;
            set {
                if(_IsSoutherlyVelocity != value) {
                    _CalculatedValues = false;
                    _IsSoutherlyVelocity = value;
                }
            }
        }

        private short? _EastWestVelocity;
        /// <summary>
        /// Gets or sets the velocity in knots in either the east or west direction.
        /// </summary>
        /// <remarks>
        /// If this is null then the east-west velocity was not transmitted in the velocity message.
        /// </remarks>
        public short? EastWestVelocity
        {
            get => _EastWestVelocity;
            set {
                if(_EastWestVelocity != value) {
                    _CalculatedValues = false;
                    _EastWestVelocity = value;
                }
            }
        }

        private bool _EastWestExceeded;
        /// <summary>
        /// Gets or sets a value indicating that the actual velocity along the east-west axis is higher than the value in <see cref="EastWestVelocity"/>.
        /// </summary>
        /// <remarks>
        /// Depending on the message the maximum velocity is either 1021 knots or 4084 knots. If this flag is set
        /// then the aircraft was travelling faster than the highest velocity that the message could convey.
        /// </remarks>
        public bool EastWestExceeded
        {
            get => _EastWestExceeded;
            set {
                if(_EastWestExceeded != value) {
                    _CalculatedValues = false;
                    _EastWestExceeded = value;
                }
            }
        }

        private short? _NorthSouthVelocity;
        /// <summary>
        /// Gets or sets the velocity in knots in either the north or south direction.
        /// </summary>
        /// <remarks>
        /// If this is null then the north-south velocity was not transmitted in the velocity message.
        /// </remarks>
        public short? NorthSouthVelocity
        {
            get => _NorthSouthVelocity;
            set {
                if(_NorthSouthVelocity != value) {
                    _CalculatedValues = false;
                    _NorthSouthVelocity = value;
                }
            }
        }

        private bool _NorthSouthExceeded;
        /// <summary>
        /// Gets or sets a value indicating that the actual velocity along the north-south axis is higher than the value in <see cref="NorthSouthVelocity"/>.
        /// </summary>
        /// <remarks>
        /// Depending on the message the maximum velocity is either 1021 knots or 4084 knots. If this flag is set
        /// then the aircraft was travelling faster than the highest velocity that the message could convey.
        /// </remarks>
        public bool NorthSouthExceeded
        {
            get => _NorthSouthExceeded;
            set {
                if(_NorthSouthExceeded != value) {
                    _CalculatedValues = false;
                    _NorthSouthExceeded = value;
                }
            }
        }

        private double? _Speed;
        /// <summary>
        /// Gets the speed in knots as calculated from the other properties.
        /// </summary>
        public double? Speed
        {
            get {
                CalculateValues();
                return _Speed;
            }
        }

        private double? _Bearing;
        /// <summary>
        /// Gets the bearing as calculated from the other properties.
        /// </summary>
        public double? Bearing
        {
            get {
                CalculateValues();
                return _Bearing;
            }
        }

        /// <summary>
        /// Calculates the velocity and bearing if the current values are out-of-date.
        /// </summary>
        private void CalculateValues()
        {
            if(!_CalculatedValues) {
                _CalculatedValues = true;
                _Speed = null;
                _Bearing = null;

                if(!NorthSouthExceeded && !EastWestExceeded) {
                    if(NorthSouthVelocity == 0 && EastWestVelocity == 0) {
                        _Speed = 0.0;
                    } else if(NorthSouthVelocity != null && EastWestVelocity != null) {
                        var vx = (double)EastWestVelocity;
                        var vy = (double)NorthSouthVelocity;
                        _Speed = CalculateVectorSpeed(vx, vy);

                        if(EastWestVelocity == 0) {
                            _Bearing = IsSoutherlyVelocity ? 180.0 : 0.0;
                        } else {
                            _Bearing = Math.Atan(vy / vx) * (180.0 / Math.PI);
                            if(IsSoutherlyVelocity) {
                                if(!_IsWesterlyVelocity) {
                                    _Bearing += 90.0;
                                } else {
                                    _Bearing = 270.0 - _Bearing;
                                }
                            } else {
                                if(IsWesterlyVelocity) {
                                    _Bearing += 270.0;
                                } else {
                                    _Bearing = 90.0 - _Bearing;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static double CalculateVectorSpeed(double eastWestVelocity, double northSouthVelocity)
        {
            return Math.Sqrt(
                  (eastWestVelocity * eastWestVelocity)
                + (northSouthVelocity * northSouthVelocity)
            );
        }

        /// <summary>
        /// Returns an English description of the velocity.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{EastWestVelocity}{(IsWesterlyVelocity ? 'W' : 'E')}{(EastWestExceeded ? "*" : "")}" +
                   $"/{NorthSouthVelocity}{(IsSoutherlyVelocity ? 'S' : 'N')}{(NorthSouthExceeded ? "*" : "")}";
        }
    }
}
