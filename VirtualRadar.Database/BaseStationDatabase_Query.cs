using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;

namespace VirtualRadar.Database
{
    public static class BaseStationDatabase_Query
    {
        public static IQueryable<KineticFlight> ApplyBaseStationFlightCriteria(
            IQueryable<KineticFlight> query,
            SearchBaseStationCriteria criteria,
            ICallsignParser callsignParser,
            int fromRow,
            int toRow,
            string sort1,
            bool sort1Ascending,
            string sort2,
            bool sort2Ascending
        )
        {
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
                query = ApplyStringFilter(query, criteria.Callsign,
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
            query = ApplyStringFilter(query, criteria.Country,
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
            query = ApplyDateRangeFilter(query, criteria.Date,
                q => q.Where(r => r.StartTime >= criteria.Date.LowerValue.Value),
                q => q.Where(r => r.StartTime <= criteria.Date.UpperValue.Value),
                q => q.Where(r => r.StartTime < criteria.Date.LowerValue.Value),
                q => q.Where(r => r.StartTime > criteria.Date.UpperValue.Value),
                q => q.Where(r => r.StartTime < criteria.Date.LowerValue.Value || r.StartTime > criteria.Date.UpperValue.Value)
            );
            query = ApplyIntRangeFilter(query, criteria.FirstAltitude,
                q => q.Where(r => r.FirstAltitude >= criteria.FirstAltitude.LowerValue.Value),
                q => q.Where(r => r.FirstAltitude <= criteria.FirstAltitude.UpperValue.Value),
                q => q.Where(r => r.FirstAltitude < criteria.FirstAltitude.LowerValue.Value),
                q => q.Where(r => r.FirstAltitude > criteria.FirstAltitude.UpperValue.Value),
                q => q.Where(r => r.FirstAltitude < criteria.FirstAltitude.LowerValue.Value || r.FirstAltitude > criteria.FirstAltitude.UpperValue.Value)
            );
            query = ApplyStringFilter(query, criteria.Icao,
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
            query = ApplyBoolFilter(query, criteria.IsEmergency,
                q => q.Where(r => r.HadEmergency == criteria.IsEmergency.Value),
                q => q.Where(r => r.HadEmergency != criteria.IsEmergency.Value)
            );
            query = ApplyIntRangeFilter(query, criteria.LastAltitude,
                q => q.Where(r => r.LastAltitude >= criteria.LastAltitude.LowerValue.Value),
                q => q.Where(r => r.LastAltitude <= criteria.LastAltitude.UpperValue.Value),
                q => q.Where(r => r.LastAltitude < criteria.LastAltitude.LowerValue.Value),
                q => q.Where(r => r.LastAltitude > criteria.LastAltitude.UpperValue.Value),
                q => q.Where(r => r.LastAltitude < criteria.LastAltitude.LowerValue.Value || r.LastAltitude > criteria.LastAltitude.UpperValue.Value)
            );
            query = ApplyStringFilter(query, criteria.Operator,
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
            query = ApplyStringFilter(query, criteria.Registration,
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
            query = ApplyStringFilter(query, criteria.Type,
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

            if(sort1 != null) {
                query = sort1Ascending
                    ? query.OrderBy(r => r.Callsign)
                    : query.OrderByDescending(r => r.Callsign);
            }

            fromRow = Math.Max(0, fromRow);
            if(fromRow > 0) {
                query = query.Skip(fromRow);
            }
            if(toRow >= fromRow) {
                query = query.Take((toRow - fromRow) + 1);
            }

            return query;
        }

        private static IQueryable<KineticFlight> ApplyStringFilter(
            IQueryable<KineticFlight> query,
            FilterString filter,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereEqual,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotEqual,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereIsNull,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereIsNotNull,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereContains,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotContains,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereStartsWith,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotStartsWith,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereEndsWith,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotEndsWith
        )
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Contains:
                        query = !filter.ReverseCondition
                            ? whereContains(query)
                            : whereNotContains(query);
                        break;
                    case FilterCondition.EndsWith:
                        query = !filter.ReverseCondition
                            ? whereEndsWith(query)
                            : whereNotEndsWith(query);
                        break;
                    case FilterCondition.StartsWith:
                        query = !filter.ReverseCondition
                            ? whereStartsWith(query)
                            : whereNotStartsWith(query);
                        break;
                    case FilterCondition.Equals:
                        var matchNull = String.IsNullOrEmpty(filter.Value);
                        if(!filter.ReverseCondition) {
                            query = matchNull
                                ? whereIsNull(query)
                                : whereEqual(query);
                        } else {
                            query = matchNull
                                ? whereIsNotNull(query)
                                : whereNotEqual(query);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return query;
        }

        private static IQueryable<KineticFlight> ApplyBoolFilter(
            IQueryable<KineticFlight> query,
            FilterBool filter,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereEqual,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotEqual
        )
        {
            if(filter != null && filter.Condition == FilterCondition.Equals) {
                query = !filter.ReverseCondition
                    ? whereEqual(query)
                    : whereNotEqual(query);
            }

            return query;
        }

        private static IQueryable<KineticFlight> ApplyRangeFilter(
            IQueryable<KineticFlight> query,
            Filter filter,
            Func<bool> filterHasLowerValue,
            Func<bool> filterHasUpperValue,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLowerAndUpper
        )
        {
            if(filter != null) {
                var hasLower = filterHasLowerValue();
                var hasUpper = filterHasUpperValue();

                if(!filter.ReverseCondition) {
                    if(hasLower) {
                        query = whereLower(query);
                    }
                    if(hasUpper) {
                        query = whereUpper(query);
                    }
                } else {
                    if(hasLower && hasUpper) {
                        query = whereNotLowerAndUpper(query);
                    } else if(hasLower) {
                        query = whereNotLower(query);
                    } else if(hasUpper) {
                        query = whereNotUpper(query);
                    }
                }
            }

            return query;
        }

        private static IQueryable<KineticFlight> ApplyDateRangeFilter(
            IQueryable<KineticFlight> query,
            FilterRange<DateTime> filter,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLowerAndUpper
        )
        {
            return ApplyRangeFilter(
                query,
                filter,
                () => filter.LowerValue != null && filter.LowerValue.Value.Year != DateTime.MinValue.Year,
                () => filter.UpperValue != null && filter.UpperValue.Value.Year != DateTime.MaxValue.Year,
                whereLower,
                whereUpper,
                whereNotLower,
                whereNotUpper,
                whereNotLowerAndUpper
            );
        }

        private static IQueryable<KineticFlight> ApplyIntRangeFilter(
            IQueryable<KineticFlight> query,
            FilterRange<int> filter,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLower,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotUpper,
            Func<IQueryable<KineticFlight>, IQueryable<KineticFlight>> whereNotLowerAndUpper
        )
        {
            return ApplyRangeFilter(
                query,
                filter,
                () => filter.LowerValue != null && filter.LowerValue.Value != int.MinValue,
                () => filter.UpperValue != null && filter.UpperValue.Value != int.MaxValue,
                whereLower,
                whereUpper,
                whereNotLower,
                whereNotUpper,
                whereNotLowerAndUpper
            );
        }
    }
}
