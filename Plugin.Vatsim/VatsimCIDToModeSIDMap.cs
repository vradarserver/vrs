// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// Manages the creation and removal of fake Mode-S ICAO identifiers for
    /// VATSIM pilot CIDs.
    /// </summary>
    static class VatsimCIDToModeSIDMap
    {
        public const int StartFakeModeSRange = 0xB00000;
        public const int EndFakeModeSRange =   0xBFFFFF;

        private static object _WriteLock = new object();
        private static volatile Dictionary<int, int> _VatsimToModeSMap = new Dictionary<int, int>();
        private static volatile Dictionary<int, int> _ModeSToVatsimMap = new Dictionary<int, int>();
        private static Dictionary<int, int> _PreferredMapping = new Dictionary<int, int>();
        private static int _NextModeSID = StartFakeModeSRange;

        /// <summary>
        /// Retrieves the mapping between a VATSIM pilot's CID and a fake aircraft ICAO code.
        /// </summary>
        /// <param name="vatsimCid"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// This will reuse fake ICAOs after a while. If a pilot CID has been previously seen and then
        /// removed then it will attempt to reassign the same fake ICAO as before, but if that ICAO is
        /// already in use then a new ICAO is generated for it.
        /// </para><para>
        /// The range of fake ICAOs is finite. If there are more pilots than there are fake ICAOs then
        /// -1 is returned.
        /// </para></remarks>
        public static int CreateOrFetchVatsimToModeSMapping(int vatsimCid)
        {
            var extant = _VatsimToModeSMap;
            if(!extant.TryGetValue(vatsimCid, out var result)) {
                lock(_WriteLock) {
                    if(!_VatsimToModeSMap.TryGetValue(vatsimCid, out result)) {
                        if(_PreferredMapping.TryGetValue(vatsimCid, out result)) {
                            if(_ModeSToVatsimMap.ContainsKey(result)) {
                                result = default;
                            }
                        }

                        var originalModeSID = _NextModeSID;

                        while(result == default) {
                            var modeSID = _NextModeSID++;
                            if(_NextModeSID > EndFakeModeSRange) {
                                _NextModeSID = StartFakeModeSRange;
                            }
                            if(!_ModeSToVatsimMap.ContainsKey(modeSID)) {
                                result = modeSID;
                            } else if(modeSID == originalModeSID) {
                                result = -1;
                            }
                        }

                        if(result != -1) {
                            var newVatsimToModeS = CollectionHelper.ShallowCopy(_VatsimToModeSMap);
                            newVatsimToModeS.Add(vatsimCid, result);

                            var newModeSToVatsim = CollectionHelper.ShallowCopy(_ModeSToVatsimMap);
                            newModeSToVatsim.Add(result, vatsimCid);

                            _VatsimToModeSMap = newVatsimToModeS;
                            _ModeSToVatsimMap = newModeSToVatsim;

                            _PreferredMapping[vatsimCid] = result;
                        }
                    }
                }
            }

            return result;
        }

        public static void RemoveVatsimToModeSMapping(int vatsimCid)
        {
            lock(_WriteLock) {
                if(_VatsimToModeSMap.TryGetValue(vatsimCid, out var modeSID)) {
                    var newVatsimToModeS = CollectionHelper.ShallowCopy(_VatsimToModeSMap);
                    newVatsimToModeS.Remove(vatsimCid);

                    var newModeSToVatsim = CollectionHelper.ShallowCopy(_ModeSToVatsimMap);
                    newModeSToVatsim.Remove(modeSID);

                    _VatsimToModeSMap = newVatsimToModeS;
                    _ModeSToVatsimMap = newModeSToVatsim;
                }
            }
        }
    }
}
