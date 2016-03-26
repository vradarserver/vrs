// Copyright © 2016 onwards, Andrew Whewell
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
using KdTree;
using KdTree.Math;
using VirtualRadar.Interface;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAirPressureLookup"/>.
    /// </summary>
    class AirPressureLookup : IAirPressureLookup
    {
        /// <summary>
        /// The kd-tree that holds all of the air pressure records indexed by latitude
        /// and longitude. Do not use the field directly unless you have _SyncLock
        /// locked - take a copy of the reference and use that if you're just reading
        /// it. Never write to the field directly, always lock _SyncLock and then
        /// overwrite it with a new copy.
        /// </summary>
        private KdTree<float, AirPressure> _AirPressures = null;

        /// <summary>
        /// Locks _AirPressures for writing.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime FetchTimeUtc { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int CountAirPressuresLoaded { get; private set; }

        private static IAirPressureLookup _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAirPressureLookup Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new AirPressureLookup();
                }
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="airPressures"></param>
        /// <param name="fetchTimeUtc"></param>
        public void LoadAirPressures(IEnumerable<AirPressure> airPressures, DateTime fetchTimeUtc)
        {
            if(airPressures == null) throw new ArgumentNullException("airPressures");
            FetchTimeUtc = fetchTimeUtc;

            CountAirPressuresLoaded = 0;
            var kdTree = new KdTree<float, AirPressure>(2, new GeoMath());
            foreach(var airPressure in airPressures) {
                var node = new KdTreeNode<float, AirPressure>(new float[] { airPressure.Latitude, airPressure.Longitude }, airPressure);
                kdTree.Add(node.Point, node.Value);
            }

            lock(_SyncLock) {
                _AirPressures = kdTree;
            }
            CountAirPressuresLoaded = kdTree.Count;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public AirPressure FindClosest(double latitude, double longitude)
        {
            AirPressure result = null;

            var kdTree = _AirPressures;
            if(kdTree != null) {
                var nearestPoints = kdTree.GetNearestNeighbours(new float[] { (float)latitude, (float)longitude}, 1);
                var nearestPoint = nearestPoints.FirstOrDefault();
                result = nearestPoint == null ? null : nearestPoint.Value;
            }

            return result;
        }
    }
}
