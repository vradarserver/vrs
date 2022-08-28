// Copyright © 2022 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.Vatsim.WebAdmin
{
    public class OptionsModel
    {
        public long DataVersion { get; set; }

        public bool Enabled { get; set; }

        public int RefreshIntervalSeconds { get; set; }

        public bool AssumeSlowAircraftAreOnGround { get; set; }

        public int SlowAircraftThresholdSpeedKnots { get; set; }

        public bool InferModelFromModelType { get; set; }

        public bool ShowInvalidRegistrations { get; set; }

        public List<GeofenceFeedOptionModel> GeofencedFeeds { get; private set; } = new List<GeofenceFeedOptionModel>();

        public EnumModel[] CentreOnTypes { get; }

        public EnumModel[] DistanceUnitTypes { get; }

        public OptionsModel()
        {
            CentreOnTypes = EnumModel.CreateFromEnum<GeofenceCentreOn>(r => DescribeVatsim.GeofenceCentreOn(r));
            DistanceUnitTypes = EnumModel.CreateFromEnum<DistanceUnit>(r => Describe.DistanceUnit(r));
        }

        public static OptionsModel FromOption(Options option)
        {
            var result = new OptionsModel() {
                DataVersion =                       option.DataVersion,
                Enabled =                           option.Enabled,
                RefreshIntervalSeconds =            option.RefreshIntervalSeconds,
                AssumeSlowAircraftAreOnGround =     option.AssumeSlowAircraftAreOnGround,
                SlowAircraftThresholdSpeedKnots =   option.SlowAircraftThresholdSpeedKnots,
                InferModelFromModelType =           option.InferModelFromModelType,
                ShowInvalidRegistrations =          option.ShowInvalidRegistrations,
            };
            result.GeofencedFeeds.AddRange(
                option.GeofencedFeeds.Select(r => GeofenceFeedOptionModel.FromOption(r))
            );

            return result;
        }

        public Options ToOption()
        {
            var result = new Options() {
                DataVersion =                       DataVersion,
                Enabled =                           Enabled,
                RefreshIntervalSeconds =            RefreshIntervalSeconds,
                AssumeSlowAircraftAreOnGround =     AssumeSlowAircraftAreOnGround,
                SlowAircraftThresholdSpeedKnots =   SlowAircraftThresholdSpeedKnots,
                InferModelFromModelType =           InferModelFromModelType,
                ShowInvalidRegistrations =          ShowInvalidRegistrations
            };
            result.GeofencedFeeds.AddRange(
                GeofencedFeeds.Select(r => r.ToOption())
            );

            return result;
        }
    }

    public class GeofenceFeedOptionModel
    {
        public Guid? ID { get; set; }

        public string FeedName { get; set; }

        public int CentreOn { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public int? PilotCid { get; set; }

        public string AirportCode { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public int DistanceUnit { get; set; }

        public static GeofenceFeedOptionModel FromOption(GeofenceFeedOption option)
        {
            return new GeofenceFeedOptionModel() {
                ID =            option.ID,
                FeedName =      option.FeedName,
                CentreOn =      (int)option.CentreOn,
                Latitude =      option.Latitude,
                Longitude =     option.Longitude,
                PilotCid =      option.PilotCid,
                AirportCode =   option.AirportCode,
                Width =         option.Width,
                Height =        option.Height,
                DistanceUnit =  (int)option.DistanceUnit,
            };
        }

        public GeofenceFeedOption ToOption()
        {
            return new GeofenceFeedOption() {
                ID =            ID ?? Guid.NewGuid(),
                FeedName =      FeedName,
                CentreOn =      (GeofenceCentreOn)CentreOn,
                Latitude =      Latitude,
                Longitude =     Longitude,
                PilotCid =      PilotCid,
                AirportCode =   AirportCode,
                Width =         Width,
                Height =        Height,
                DistanceUnit =  (DistanceUnit)DistanceUnit,
            };
        }
    }

    public class SaveOutcomeModel
    {
        public string Outcome { get; set; }

        public OptionsModel Options { get; set; }

        public SaveOutcomeModel(string outcome, OptionsModel options)
        {
            Outcome = outcome;
            Options = options;
        }
    }
}
