using Moq;
using VirtualRadar.Interface.StandingData;

namespace Test.Framework
{
    public class MockStandingDataManager : Mock<IStandingDataManager>
    {
        public List<AircraftType> AllAircraftTypes { get; } = new();

        public List<Airline> AllAirlines { get; } = new();

        public List<Airport> AllAirports { get; } = new();

        public Dictionary<string, CodeBlock> AllCodeBlocksByIcao24 { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, Route> AllRoutesByCallsign { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool CodeBlocksLoaded { get; set; } = true;

        public MockStandingDataManager(bool setupAll = true)
        {
            if(setupAll) {
                SetupAll();
            }
        }

        public void SetupAll()
        {
            SetupGet(r => r.CodeBlocksLoaded)
            .Returns(() => CodeBlocksLoaded);

            Setup(r => r.FindAircraftType(It.IsAny<string>()))
            .Returns((string type) => {
                return AllAircraftTypes.FirstOrDefault(aircraftType => aircraftType.Type == type);
            });

            Setup(r => r.FindAirlinesForCode(It.IsAny<string>()))
            .Returns((string code) => {
                var isIcao = code?.Length == 3;
                return AllAirlines
                    .Where(r => isIcao ? r.IcaoCode == code : r.IataCode == code);
            });

            Setup(r => r.FindAirportForCode(It.IsAny<string>()))
            .Returns((string code) => {
                var isIcao = code?.Length == 4;
                return AllAirports
                    .FirstOrDefault(r => isIcao ? r.IcaoCode == code : r.IataCode == code);
            });

            Setup(r => r.FindCodeBlock(It.IsAny<string>()))
            .Returns((string icao24) => {
                AllCodeBlocksByIcao24.TryGetValue(icao24 ?? "", out var result);
                return result;
            });

            Setup(r => r.FindRoute(It.IsAny<string>()))
            .Returns((string callsign) => {
                AllRoutesByCallsign.TryGetValue(callsign ?? "", out var result);
                return result;
            });

            Setup(r => r.Lock(It.IsAny<Action<IStandingDataManager>>()))
            .Callback((Action<IStandingDataManager> callback) => callback(Object));
        }
    }
}
