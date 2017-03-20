// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class PolarPlotJsonTests
    {
        private PolarPlotJson _Json;

        [TestInitialize]
        public void TestInitialise()
        {
            _Json = new PolarPlotJson();
        }

        [TestMethod]
        public void PolarPlotJson_Constructor_Initialises_To_Known_Values()
        {
            TestUtilities.TestProperty(_Json, r => r.Latitude, 0F, 2F);
            TestUtilities.TestProperty(_Json, r => r.Longitude, 0F, 99F);
        }

        [TestMethod]
        public void PolarPlotJson_ToModel_Returns_Copy_Of_Polar_Plot()
        {
            var model = PolarPlotJson.ToModel(new PolarPlot() {
                Altitude =  12345,
                Angle =     70,
                Distance =  14.3,
                Latitude =  51.3412,
                Longitude = -0.6234,
            });

            Assert.AreEqual(51.3412F, model.Latitude, 0.1F);
            Assert.AreEqual(-0.6234F, model.Longitude, 0.1F);
        }

        [TestMethod]
        public void PolarPlotJson_ToModel_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(PolarPlotJson.ToModel(null));
        }
    }
}
