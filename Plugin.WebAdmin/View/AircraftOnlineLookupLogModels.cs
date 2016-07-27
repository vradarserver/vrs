using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog
{
    public class ViewModel
    {
        public LogEntry[] LogEntries { get; private set; }

        public ViewModel(IEnumerable<AircraftOnlineLookupLogEntry> logEntries)
        {
            LogEntries = logEntries.Select(r => new LogEntry(r)).ToArray();
        }
    }

    public class LogEntry
    {
        public string Time { get; private set; }

        public string Icao { get; private set; }

        public string Registration { get; private set; }

        public string Country { get; private set; }

        public string Manufacturer { get; private set; }

        public string Model { get; private set; }

        public string ModelIcao { get; private set; }

        public string Operator { get; private set; }

        public string OperatorIcao { get; private set; }

        public string Serial { get; private set; }

        public int? YearBuilt { get; private set; }

        public LogEntry(AircraftOnlineLookupLogEntry entry)
        {
            Time = String.Format("{0:HH:mm:ss}", entry.ResponseUtc.ToLocalTime());
            Icao = entry.Icao;

            var detail = entry.Detail;
            if(detail != null) {
                Registration =  detail.Registration;
                Country =       detail.Country;
                Manufacturer =  detail.Manufacturer;
                Model =         detail.Model;
                ModelIcao =     detail.ModelIcao;
                Operator =      detail.Operator;
                OperatorIcao =  detail.OperatorIcao;
                Serial =        detail.Serial;
                YearBuilt =     detail.YearBuilt;
            }
        }
    }
}
