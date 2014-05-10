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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace Test.Framework
{
    /// <summary>
    /// A set of static methods that help with tests that involve <see cref="IAircraft"/>.
    /// </summary>
    public static class AircraftTestHelper
    {
        /// <summary>
        /// Returns a dummy value that can be copied into any of the properties on an <see cref="IAircraft"/> object.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is really quite an esoteric method - it just generates dummy values to set properties to in tests
        /// that are iterating over all of the properties of an <see cref="IAircraft"/> and don't really mind what the
        /// value of the property is, just so long as it has a value. There are only a couple of tests that use this
        /// but they're in different test libraries and so this method needed to be in a library common to both.
        /// </remarks>
        public static object GenerateAircraftPropertyValue(Type propertyType)
        {
            return propertyType == typeof(bool) || propertyType == typeof(bool?) ? true :
                   propertyType == typeof(float) || propertyType == typeof(float?) ? 1F :
                   propertyType == typeof(double) || propertyType == typeof(double?) ? 1.0 :
                   propertyType == typeof(int) || propertyType == typeof(int?) ? 1 :
                   propertyType == typeof(long) || propertyType == typeof(long?) ? 1L :
                   propertyType == typeof(DateTime) || propertyType == typeof(DateTime?) ? DateTime.Now :
                   propertyType == typeof(WakeTurbulenceCategory) ? WakeTurbulenceCategory.Heavy :
                   propertyType == typeof(Species) ? Species.Helicopter :
                   propertyType == typeof(EngineType) ? EngineType.Turboprop :
                   propertyType == typeof(SpeedType) ? SpeedType.IndicatedAirSpeed :
                   propertyType == typeof(List<Coordinate>) ? new Coordinate(1L, 2L, 3f, 4f, 5f) :
                   (object)"XYZ";
        }
    }
}
