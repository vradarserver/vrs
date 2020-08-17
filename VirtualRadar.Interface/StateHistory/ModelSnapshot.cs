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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.StateHistory
{
    /// <summary>
    /// Describes a snapshot of an aircraft model in the state history.
    /// </summary>
    public class ModelSnapshot : SnapshotRecord
    {
        /// <summary>
        /// Gets or sets the unique ID of the snapshot in the database.
        /// </summary>
        public long ModelSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the ICAO 8643 type code.
        /// </summary>
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the ID of the manufacturer snapshot.
        /// </summary>
        public long? ManufacturerSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the WTC snapshot.
        /// </summary>
        public long? WakeTurbulenceCodeSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the engine type snapshot.
        /// </summary>
        public long? EngineTypeSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the engine placement snapshot.
        /// </summary>
        public long? EnginePlacementSnapshotID { get; set; }

        /// <summary>
        /// Gets or sets the number of engines. Note that ICAO use letters for some engine counts.
        /// </summary>
        public string NumberOfEngines { get; set; }

        /// <summary>
        /// Gets or sets the species snapshot ID.
        /// </summary>
        public long? SpeciesSnapshotID { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override byte[] FingerprintProperties()
        {
            return TakeFingerprint(
                Icao,
                ModelName,
                NumberOfEngines,
                ManufacturerSnapshotID,
                WakeTurbulenceCodeSnapshotID,
                EngineTypeSnapshotID,
                EnginePlacementSnapshotID,
                SpeciesSnapshotID
            );
        }

        /// <summary>
        /// Returns the fingerprint derived from component parts.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="modelName"></param>
        /// <param name="numberOfEngines"></param>
        /// <param name="manufacturerSnapshotID"></param>
        /// <param name="wakeTurbulenceCategorySnapshotID"></param>
        /// <param name="engineTypeSnapshotID"></param>
        /// <param name="enginePlacementSnapshotID"></param>
        /// <param name="speciesSnapshotID"></param>
        /// <returns></returns>
        public static byte[] TakeFingerprint(
            string icao,
            string modelName,
            string numberOfEngines,
            long? manufacturerSnapshotID,
            long? wakeTurbulenceCategorySnapshotID,
            long? engineTypeSnapshotID,
            long? enginePlacementSnapshotID,
            long? speciesSnapshotID
        ) => Sha1Fingerprint.CreateFingerprintFromObjects(
            icao,
            modelName,
            numberOfEngines,
            manufacturerSnapshotID,
            wakeTurbulenceCategorySnapshotID,
            engineTypeSnapshotID,
            enginePlacementSnapshotID,
            speciesSnapshotID
        );

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{FingerprintHex}] {Icao}: {ModelName}";
    }
}
