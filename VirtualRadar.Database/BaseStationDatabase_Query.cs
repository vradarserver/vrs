using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;

namespace VirtualRadar.Database
{
    public static class BaseStationDatabase_Query
    {
        private static SortColumnClauses<KineticFlight>[] _FlightSortClauses = new SortColumnClauses<KineticFlight>[] {
            new("callsign",         q => q.OrderBy(r => r.Callsign),                    q => q.OrderByDescending(r => r.Callsign),                  q => q.ThenBy(r => r.Callsign),                     q => q.ThenByDescending(r => r.Callsign)),
            new("country",          q => q.OrderBy(r => r.Aircraft.Country),            q => q.OrderByDescending(r => r.Aircraft.Country),          q => q.ThenBy(r => r.Aircraft.Country),             q => q.ThenByDescending(r => r.Aircraft.Country)),
            new("date",             q => q.OrderBy(r => r.StartTime),                   q => q.OrderByDescending(r => r.StartTime),                 q => q.ThenBy(r => r.StartTime),                    q => q.ThenByDescending(r => r.StartTime)),
            new("firstaltitude",    q => q.OrderBy(r => r.FirstAltitude),               q => q.OrderByDescending(r => r.FirstAltitude),             q => q.ThenBy(r => r.FirstAltitude),                q => q.ThenByDescending(r => r.FirstAltitude)),
            new("icao",             q => q.OrderBy(r => r.Aircraft.ModeS),              q => q.OrderByDescending(r => r.Aircraft.ModeS),            q => q.ThenBy(r => r.Aircraft.ModeS),               q => q.ThenByDescending(r => r.Aircraft.ModeS)),
            new("lastaltitude",     q => q.OrderBy(r => r.LastAltitude),                q => q.OrderByDescending(r => r.LastAltitude),              q => q.ThenBy(r => r.LastAltitude),                 q => q.ThenByDescending(r => r.LastAltitude)),
            new("model",            q => q.OrderBy(r => r.Aircraft.Type),               q => q.OrderByDescending(r => r.Aircraft.Type),             q => q.ThenBy(r => r.Aircraft.Type),                q => q.ThenByDescending(r => r.Aircraft.Type)),
            new("operator",         q => q.OrderBy(r => r.Aircraft.RegisteredOwners),   q => q.OrderByDescending(r => r.Aircraft.RegisteredOwners), q => q.ThenBy(r => r.Aircraft.RegisteredOwners),    q => q.ThenByDescending(r => r.Aircraft.RegisteredOwners)),
            new("reg",              q => q.OrderBy(r => r.Aircraft.Registration),       q => q.OrderByDescending(r => r.Aircraft.Registration),     q => q.ThenBy(r => r.Aircraft.Registration),        q => q.ThenByDescending(r => r.Aircraft.Registration)),
            new("type",             q => q.OrderBy(r => r.Aircraft.ICAOTypeCode),       q => q.OrderByDescending(r => r.Aircraft.ICAOTypeCode),     q => q.ThenBy(r => r.Aircraft.ICAOTypeCode),        q => q.ThenByDescending(r => r.Aircraft.ICAOTypeCode)),
        };

        public static IQueryable<KineticFlight> ApplyBaseStationFlightCriteria(
            IQueryable<KineticFlight> query,
            SearchBaseStationCriteria criteria,
            KineticAircraft constrainToAircraft,
            ICallsignParser callsignParser,
            int fromRow,
            int toRow,
            string sort1,
            bool sort1Ascending,
            string sort2,
            bool sort2Ascending
        )
        {
            if(constrainToAircraft != null) {
                query = query.Where(r => r.AircraftID == constrainToAircraft.AircraftID);
            }

            if(   criteria.UseAlternateCallsigns
               && criteria.Callsign != null
               && criteria.Callsign.Condition == FilterCondition.Equals
               && !String.IsNullOrEmpty(criteria.Callsign.Value)
            ) {
                var alternates = callsignParser.GetAllAlternateCallsigns(criteria.Callsign.Value);
                query = !criteria.Callsign.ReverseCondition
                    ? query.Where(r => alternates.Contains(r.Callsign))
                    : query.Where(r => !alternates.Contains(r.Callsign));
            } else {
                query = query.WhereStringFilter(criteria.Callsign,
                    q => q.Where(r => r.Callsign == criteria.Callsign.Value),
                    q => q.Where(r => r.Callsign != null && r.Callsign != criteria.Callsign.Value),
                    q => q.Where(r => (r.Callsign ?? "") == ""),
                    q => q.Where(r => (r.Callsign ?? "") != ""),
                    q => q.Where(r => r.Callsign != null && r.Callsign.Contains(criteria.Callsign.Value)),
                    q => q.Where(r => r.Callsign != null && !r.Callsign.Contains(criteria.Callsign.Value)),
                    q => q.Where(r => r.Callsign != null && r.Callsign.StartsWith(criteria.Callsign.Value)),
                    q => q.Where(r => r.Callsign != null && !r.Callsign.StartsWith(criteria.Callsign.Value)),
                    q => q.Where(r => r.Callsign != null && r.Callsign.EndsWith(criteria.Callsign.Value)),
                    q => q.Where(r => r.Callsign != null && !r.Callsign.EndsWith(criteria.Callsign.Value))
                );
            }
            query = query.WhereStringFilter(criteria.Country,
                q => q.Where(r => r.Aircraft.ModeSCountry == criteria.Country.Value),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry != criteria.Country.Value),
                q => q.Where(r => (r.Aircraft.ModeSCountry ?? "") == ""),
                q => q.Where(r => (r.Aircraft.ModeSCountry ?? "") != ""),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry.Contains(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && !r.Aircraft.ModeSCountry.Contains(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry.StartsWith(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && !r.Aircraft.ModeSCountry.StartsWith(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry.EndsWith(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && !r.Aircraft.ModeSCountry.EndsWith(criteria.Country.Value))
            );
            query = query.WhereDateRangeFilter(criteria.Date,
                q => q.Where(r => r.StartTime >= criteria.Date.LowerValue.Value),
                q => q.Where(r => r.StartTime <= criteria.Date.UpperValue.Value),
                q => q.Where(r => r.StartTime < criteria.Date.LowerValue.Value),
                q => q.Where(r => r.StartTime > criteria.Date.UpperValue.Value),
                q => q.Where(r => r.StartTime < criteria.Date.LowerValue.Value || r.StartTime > criteria.Date.UpperValue.Value)
            );
            query = query.WhereIntRangeFilter(criteria.FirstAltitude,
                q => q.Where(r => r.FirstAltitude >= criteria.FirstAltitude.LowerValue.Value),
                q => q.Where(r => r.FirstAltitude <= criteria.FirstAltitude.UpperValue.Value),
                q => q.Where(r => r.FirstAltitude < criteria.FirstAltitude.LowerValue.Value),
                q => q.Where(r => r.FirstAltitude > criteria.FirstAltitude.UpperValue.Value),
                q => q.Where(r => r.FirstAltitude < criteria.FirstAltitude.LowerValue.Value || r.FirstAltitude > criteria.FirstAltitude.UpperValue.Value)
            );
            query = query.WhereStringFilter(criteria.Icao,
                q => q.Where(r => r.Aircraft.ModeS == criteria.Icao.Value),
                q => q.Where(r => r.Aircraft.ModeS != criteria.Icao.Value),
                q => q.Where(r => (r.Aircraft.ModeS ?? "") == ""),
                q => q.Where(r => (r.Aircraft.ModeS ?? "") != ""),
                q => q.Where(r => r.Aircraft.ModeS.Contains(criteria.Icao.Value)),
                q => q.Where(r => !r.Aircraft.ModeS.Contains(criteria.Icao.Value)),
                q => q.Where(r => r.Aircraft.ModeS.StartsWith(criteria.Icao.Value)),
                q => q.Where(r => !r.Aircraft.ModeS.StartsWith(criteria.Icao.Value)),
                q => q.Where(r => r.Aircraft.ModeS.EndsWith(criteria.Icao.Value)),
                q => q.Where(r => !r.Aircraft.ModeS.EndsWith(criteria.Icao.Value))
            );
            query = query.WhereBoolFilter(criteria.IsEmergency,
                q => q.Where(r => r.HadEmergency == criteria.IsEmergency.Value),
                q => q.Where(r => r.HadEmergency != criteria.IsEmergency.Value)
            );
            query = query.WhereIntRangeFilter(criteria.LastAltitude,
                q => q.Where(r => r.LastAltitude >= criteria.LastAltitude.LowerValue.Value),
                q => q.Where(r => r.LastAltitude <= criteria.LastAltitude.UpperValue.Value),
                q => q.Where(r => r.LastAltitude < criteria.LastAltitude.LowerValue.Value),
                q => q.Where(r => r.LastAltitude > criteria.LastAltitude.UpperValue.Value),
                q => q.Where(r => r.LastAltitude < criteria.LastAltitude.LowerValue.Value || r.LastAltitude > criteria.LastAltitude.UpperValue.Value)
            );
            query = query.WhereStringFilter(criteria.Operator,
                q => q.Where(r => r.Aircraft.RegisteredOwners == criteria.Operator.Value),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners != criteria.Operator.Value),
                q => q.Where(r => (r.Aircraft.RegisteredOwners ?? "") == ""),
                q => q.Where(r => (r.Aircraft.RegisteredOwners ?? "") != ""),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners.Contains(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && !r.Aircraft.RegisteredOwners.Contains(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners.StartsWith(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && !r.Aircraft.RegisteredOwners.StartsWith(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners.EndsWith(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && !r.Aircraft.RegisteredOwners.EndsWith(criteria.Operator.Value))
            );
            query = query.WhereStringFilter(criteria.Registration,
                q => q.Where(r => r.Aircraft.Registration == criteria.Registration.Value),
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration != criteria.Registration.Value),
                q => q.Where(r => (r.Aircraft.Registration ?? "") == ""),
                q => q.Where(r => (r.Aircraft.Registration ?? "") != ""),
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration.Contains(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && !r.Aircraft.Registration.Contains(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration.StartsWith(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && !r.Aircraft.Registration.StartsWith(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration.EndsWith(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && !r.Aircraft.Registration.EndsWith(criteria.Registration.Value))
            );
            query = query.WhereStringFilter(criteria.Type,
                q => q.Where(r => r.Aircraft.ICAOTypeCode == criteria.Type.Value),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode != criteria.Type.Value),
                q => q.Where(r => (r.Aircraft.ICAOTypeCode ?? "") == ""),
                q => q.Where(r => (r.Aircraft.ICAOTypeCode ?? "") != ""),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode.Contains(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && !r.Aircraft.ICAOTypeCode.Contains(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode.StartsWith(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && !r.Aircraft.ICAOTypeCode.StartsWith(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode.EndsWith(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && !r.Aircraft.ICAOTypeCode.EndsWith(criteria.Type.Value))
            );

            query = query.AddSortConditionSorting(
                new SortCondition[] {
                    new(sort1, sort1Ascending),
                    new(sort2, sort2Ascending),
                },
                _FlightSortClauses
            );
            query = query.AddSkipAndTake(fromRow, toRow);

            return query;
        }
    }
}
