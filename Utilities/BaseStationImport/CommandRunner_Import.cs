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
using VirtualRadar.Interface;

namespace BaseStationImport
{
    /// <summary>
    /// Handles execution of the import command.
    /// </summary>
    class CommandRunner_Import : CommandRunner
    {
        private ConsoleProgress _Progress;

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var source = Engine.Build(Options.Source);
            var destination = Engine.Build(Options.Destination);

            ValidateDatabaseEngine(Options.Source, source);
            ValidateDatabaseEngine(Options.Destination, destination);

            var targetDescription = new StringBuilder(Options.Destination.ToString());
            if(!Options.SuppressSchemaUpdate) {
                targetDescription.Append(" (apply schema)");
            } else {
                targetDescription.Append(" (no schema update)");
            }
            var aircraftAction = Options.ImportAircraft ? "Import / Update" : Options.ImportFlights ? "Load from target" : "Ignored";
            var locationsAction = Options.ImportLocations ? "Import / Update" : Options.ImportSessions || Options.ImportFlights ? "Load from target" : "Ignored";
            var sessionsAction = Options.ImportSessions ? "Import / Update" : Options.ImportFlights ? "Load from target" : "Ignored";
            var flightsAction = Options.ImportFlights ? $"Import / Update {Describe.IsoDate(Options.EarliestFlight)} to {Describe.IsoDate(Options.LatestFlight)}" : "Ignored";

            Console.WriteLine($"BaseStation Import");
            Console.WriteLine($"  Source:    {Options.Source}");
            Console.WriteLine($"  Target:    {targetDescription}");
            Console.WriteLine($"  Aircraft:  {aircraftAction}");
            Console.WriteLine($"  Locations: {locationsAction}");
            Console.WriteLine($"  Sessions:  {sessionsAction}");
            Console.WriteLine($"  Flights:   {flightsAction}");
            Console.WriteLine();

            var importer = new BaseStationImporter() {
                ImportAircraft =        Options.ImportAircraft,
                ImportFlights =         Options.ImportFlights,
                ImportLocations =       Options.ImportLocations,
                ImportSessions =        Options.ImportSessions,
                EarliestFlight =        Options.EarliestFlight,
                LatestFlight =          Options.LatestFlight,
                Source =                source.CreateRepository(Options.Source),
                Destination =           destination.CreateRepository(Options.Destination),
                SuppressSchemaUpdate =  Options.SuppressSchemaUpdate,
            };
            importer.TableChanged += Importer_TableChanged;
            importer.ProgressChanged += Importer_ProgressChanged;

            importer.Import();

            return true;
        }

        private void ValidateDatabaseEngine(DatabaseEngineOptions engineOptions, Engine engine)
        {
            var direction = engineOptions.IsSource ? "source" : "destination";
            if(engine == null) {
                OptionsParser.Usage($"Missing {direction} database engine type");
            }

            var validationErrors = engine.ValidateOptions(engineOptions);
            if(validationErrors.Length > 0) {
                var joinedErrors = String.Join(Environment.NewLine, validationErrors);
                OptionsParser.Usage(joinedErrors);
            }
        }

        private void Importer_TableChanged(object sender, EventArgs<string> e)
        {
            _Progress = new ConsoleProgress();
            _Progress.StartProgress();
        }

        private void Importer_ProgressChanged(object sender, ProgressEventArgs e)
        {
            _Progress.UpdateProgress(e.CurrentItem, e.TotalItems, e.Caption);
        }
    }
}
