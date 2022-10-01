using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.ModeS
{
    /// <summary>
    /// Converts between a Mode-S code and a registration. Only a handful of
    /// countries use an algorithm to convert between registration and Mode-S
    /// ICAO code, and even fewer publish the algorithm, so this cannot be
    /// used to figure out registrations for all aircraft.
    /// </summary>
    public static class RegistrationConverter
    {
        /// <summary>
        /// Returns null if the registration cannot be converted
        /// to an ICAO, otherwise returns the ICAO formatted as
        /// a six digit hex string.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public static string RegistrationToModeS(string registration)
        {
            var icao = 0;

            for(var idx = 1;idx < registration.Length;++idx) {
                var ch = registration[idx];
                switch(idx - 1) {
                    case 0:
                        icao = 1;
                        break;
                    case 1:
                        var posnValue = Base64ATo9(ch);
                        icao += posnValue + (24 * (posnValue - 1));
                        break;
                    case 2:
                        icao += Base64ATo9(ch);
                        break;
                }
            }

            return (icao + (0xA00000)).ToString("X6");
        }

        static int Base64ATo9(char ch)
        {
            // Values are:
            // A = 1
            // B = 2 (etc.)
            // H = 8
            // (skip I)
            // J = 9 (etc.)
            // N = 13
            // ... (skip O)
            // P = 14 (etc.)
            // Z = 24
            // 0 = 25 (etc.)
            // 9 = 34
            //
            // Unknown characters return 0, charset is always ASCII

            return (ch >= '0' && ch <= '9')
                ? (ch - '0') + 25
                : (ch < 'A' || ch > 'Z')
                    ? 0
                    : (ch - 'A') + (ch < 'I' ? 1 : ch < 'O' ? 0 : -1);
        }
    }
}
