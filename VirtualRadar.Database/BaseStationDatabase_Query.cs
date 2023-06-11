using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;

namespace VirtualRadar.Database
{
    public static class BaseStationDatabase_Query
    {
        public static IQueryable<KineticFlight> ApplyBaseStationFlightCriteria(
            IQueryable<KineticFlight> query,
            SearchBaseStationCriteria criteria
        )
        {
            query = ApplyFilter(query, criteria.Callsign,
                q => q.Where(r => r.Callsign != null && r.Callsign.Contains(criteria.Callsign.Value)),
                q => q.Where(r => r.Callsign != null && !r.Callsign.Contains(criteria.Callsign.Value)),
                q => q.Where(r => r.Callsign == criteria.Callsign.Value),
                q => q.Where(r => r.Callsign != null && r.Callsign != criteria.Callsign.Value)
            );
            query = ApplyFilter(query, criteria.Country,
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry.Contains(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && !r.Aircraft.ModeSCountry.Contains(criteria.Country.Value)),
                q => q.Where(r => r.Aircraft.ModeSCountry == criteria.Country.Value),
                q => q.Where(r => r.Aircraft.ModeSCountry != null && r.Aircraft.ModeSCountry != criteria.Country.Value)
            );
            query = ApplyFilter(query, criteria.Icao,
                q => q.Where(r => r.Aircraft.ModeS.Contains(criteria.Icao.Value)),
                q => q.Where(r => !r.Aircraft.ModeS.Contains(criteria.Icao.Value)),
                q => q.Where(r => r.Aircraft.ModeS == criteria.Icao.Value),
                q => q.Where(r => r.Aircraft.ModeS != criteria.Icao.Value)
            );
            query = ApplyFilter(query, criteria.IsEmergency,
                null,
                null,
                q => q.Where(r => r.HadEmergency == criteria.IsEmergency.Value),
                q => q.Where(r => r.HadEmergency != criteria.IsEmergency.Value)
            );
            query = ApplyFilter(query, criteria.Operator,
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners.Contains(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && !r.Aircraft.RegisteredOwners.Contains(criteria.Operator.Value)),
                q => q.Where(r => r.Aircraft.RegisteredOwners == criteria.Operator.Value),
                q => q.Where(r => r.Aircraft.RegisteredOwners != null && r.Aircraft.RegisteredOwners != criteria.Operator.Value)
            );
            query = ApplyFilter(query, criteria.Registration,
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration.Contains(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration != null && !r.Aircraft.Registration.Contains(criteria.Registration.Value)),
                q => q.Where(r => r.Aircraft.Registration == criteria.Registration.Value),
                q => q.Where(r => r.Aircraft.Registration != null && r.Aircraft.Registration != criteria.Registration.Value)
            );
            query = ApplyFilter(query, criteria.Type,
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode.Contains(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && !r.Aircraft.ICAOTypeCode.Contains(criteria.Type.Value)),
                q => q.Where(r => r.Aircraft.ICAOTypeCode == criteria.Type.Value),
                q => q.Where(r => r.Aircraft.ICAOTypeCode != null && r.Aircraft.ICAOTypeCode != criteria.Type.Value)
            );

            return query;
        }

        private static IQueryable<KineticFlight> ApplyFilter(
            IQueryable<KineticFlight> query,
            Filter filter,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereContains,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotContains,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereEqual,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotEqual
        )
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Contains:
                        if(!filter.ReverseCondition) {
                            query = whereContains(query);
                        } else {
                            query = whereNotContains(query);
                        }
                        break;
                    case FilterCondition.Equals:
                        if(!filter.ReverseCondition) {
                            query = whereEqual(query);
                        } else {
                            query = whereNotEqual(query);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return query;
        }
    }
}
