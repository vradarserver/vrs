// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Database.SQLite.StandingData
{
    static class ModelToPublicConverter
    {
        public static AircraftType ToAircraftType(AircraftTypeModel model)
        {
            var result = model == null
                ? (AircraftType)null
                : new() {
                      EnginePlacement =        (EnginePlacement)model.EnginePlacementId,
                      Engines =                model.Engines ?? "",
                      EngineType =             (EngineType)model.EngineTypeId,
                      Species =                (Species)model.SpeciesId,
                      Type =                   model.Icao,
                      WakeTurbulenceCategory = (WakeTurbulenceCategory)model.WakeTurbulenceId,
                };

            if(result != null) {
                foreach(var manufacturerModel in model.AircraftTypeModels.Select(r => r.Model)) {
                    if(!String.IsNullOrWhiteSpace(manufacturerModel.Name)) {
                        result.Models.Add(manufacturerModel.Name);
                    }
                    if(!String.IsNullOrWhiteSpace(manufacturerModel.Manufacturer?.Name)) {
                        result.Manufacturers.Add(manufacturerModel.Manufacturer.Name);
                    }
                }
            }

            return result;
        }

        public static AircraftType ToAircraftType(string icao, Species species)
        {
            return new() {
                Type =      icao,
                Species =   species,
            };
        }

        public static Airline ToAirline(OperatorModel model)
        {
            return model == null
                ? null
                : new() {
                    CharterFlightPattern =      model.CharterFlightPattern,
                    IataCode =                  model.Iata,
                    IcaoCode =                  model.Icao,
                    Name =                      model.Name,
                    PositioningFlightPattern =  model.PositioningFlightPattern,
                };
        }

        public static Airport ToAirport(AirportModel model)
        {
            return model == null
                ? null
                : new() {
                    AltitudeFeet =  model.Altitude,
                    Country =       model.Country.Name,
                    IataCode =      model.Iata,
                    IcaoCode =      model.Icao,
                    Latitude =      model.Latitude,
                    Longitude =     model.Longitude,
                    Name =          model.Name,
                };
        }

        public static Route ToRoute(RouteModel model)
        {
            var result = model == null
                ? (Route)null
                : new() {
                    From =      ToAirport(model.FromAirport),
                    To =        ToAirport(model.ToAirport),
                };

            if(result != null) {
                result.Stopovers.AddRange(
                    model.RouteStops.Select(r => ToAirport(r.Airport))
                );
            }

            return result;
        }
    }
}
