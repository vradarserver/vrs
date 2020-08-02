// Copyright © 2020 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Interface.XPlane
{
    /// <summary>
    /// Describes a frame sent by XPlane in response to a request for RPOS messages.
    /// </summary>
    public class XPlaneRposReply
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public float ElevationMsl { get; set; }

        public float ElevationAgl { get; set; }

        public int AltitudeFeet => (int)(ElevationMsl * 3.28084);

        public float Pitch { get; set; }

        public float Heading { get; set; }

        public float Roll { get; set; }

        public float Vx { get; set; }

        public float Vy { get; set; }
    
        public int VerticalRateFeetPerSecond => (int)(Vy * 3.28084);

        public float Vz { get; set; }

        public float GroundSpeedMetresPerSecond => (float)VectorVelocity.CalculateVectorSpeed((double)Vx, (double)Vy);

        public float GroundSpeedKnots => GroundSpeedMetresPerSecond * 1.9438445F;

        public float Pdeg { get; set; }

        public float Qdeg { get; set; }

        public float Rdeg { get; set; }

        public static XPlaneRposReply ParseResponse(byte[] response)
        {
            XPlaneRposReply result = null;

            if(response?.Length >= 69) {
                using(var memoryStream = new MemoryStream(response)) {
                    using(var binaryReader = new BinaryReader(memoryStream)) {
                        var packetType = Encoding.ASCII.GetString(
                            binaryReader.ReadBytes(5)
                        );

                        if(packetType.StartsWith("RPOS")) {
                            result = new XPlaneRposReply() {
                                Longitude =     binaryReader.ReadDouble(),
                                Latitude =      binaryReader.ReadDouble(),
                                ElevationMsl =  binaryReader.ReadSingle(),
                                ElevationAgl =  binaryReader.ReadSingle(),
                                Pitch =         binaryReader.ReadSingle(),
                                Heading =       binaryReader.ReadSingle(),
                                Roll =          binaryReader.ReadSingle(),
                                Vx =            binaryReader.ReadSingle(),
                                Vy =            binaryReader.ReadSingle(),
                                Vz =            binaryReader.ReadSingle(),
                                Pdeg =          binaryReader.ReadSingle(),
                                Qdeg =          binaryReader.ReadSingle(),
                                Rdeg =          binaryReader.ReadSingle(),
                            };
                        }
                    }
                }
            }

            return result;
        }
    }
}
