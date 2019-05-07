// Copyright © 2019 onwards, Andrew Whewell
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class TileServerSettingsTests
    {
        [TestMethod]
        public void TileServerSettings_Clone_Creates_New_Object()
        {
            foreach(var property in typeof(TileServerSettings).GetProperties()) {
                for(var pass = 0;pass < 2;++pass) {
                    var original = new TileServerSettings();

                    object expected = null;
                    switch(property.Name) {
                        case nameof(TileServerSettings.Attribution):        expected = original.Attribution = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.ClassName):          expected = original.ClassName = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.DefaultBrightness):  expected = original.DefaultBrightness = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.DefaultOpacity):     expected = original.DefaultOpacity = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.DetectRetina):       expected = original.DetectRetina = pass == 0; break;
                        case nameof(TileServerSettings.DisplayOrder):       expected = original.DisplayOrder = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.ErrorTileUrl):       expected = original.ErrorTileUrl = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.IsCustom):           expected = original.IsCustom = pass == 0; break;
                        case nameof(TileServerSettings.IsDefault):          expected = original.IsDefault = pass == 0; break;
                        case nameof(TileServerSettings.IsLayer):            expected = original.IsLayer = pass == 0; break;
                        case nameof(TileServerSettings.IsTms):              expected = original.IsTms = pass == 0; break;
                        case nameof(TileServerSettings.MapProvider):        expected = original.MapProvider = (MapProvider)(pass == 0 ? 1 : 2); break;
                        case nameof(TileServerSettings.MaxNativeZoom):      expected = original.MaxNativeZoom = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.MaxZoom):            expected = original.MaxZoom = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.MinNativeZoom):      expected = original.MinNativeZoom = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.MinZoom):            expected = original.MinZoom = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.Name):               expected = original.Name = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.Subdomains):         expected = original.Subdomains = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.Url):                expected = original.Url = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.Version):            expected = original.Version = pass == 0 ? "A" : "B"; break;
                        case nameof(TileServerSettings.ZoomOffset):         expected = original.ZoomOffset = pass == 0 ? 1 : 2; break;
                        case nameof(TileServerSettings.ZoomReverse):        expected = original.ZoomReverse = pass == 0; break;
                        case nameof(TileServerSettings.ExpandoOptions):
                            original.ExpandoOptions.Add(new TileServerSettings.ExpandoOption() {
                                Option = pass == 0 ? "A" : "B",
                                Value =  pass == 0 ? "a" : "b",
                            });
                            break;
                        default:
                            throw new NotImplementedException(property.Name);
                    }

                    var actual = (TileServerSettings)original.Clone();

                    switch(property.Name) {
                        case nameof(TileServerSettings.ExpandoOptions):
                            Assert.AreEqual(original.ExpandoOptions.Count,      actual.ExpandoOptions.Count);
                            Assert.AreNotSame(original.ExpandoOptions[0],       actual.ExpandoOptions[0]);
                            Assert.AreEqual(original.ExpandoOptions[0].Option,  actual.ExpandoOptions[0].Option);
                            Assert.AreEqual(original.ExpandoOptions[0].Value,   actual.ExpandoOptions[0].Value);
                            break;
                        default:
                            var actualValue = property.GetValue(actual, null);
                            Assert.AreEqual(expected, actualValue, "for property {0}", property.Name);
                            break;
                    }
                }
            }
        }
    }
}
