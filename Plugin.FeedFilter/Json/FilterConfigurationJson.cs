using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Plugin.FeedFilter.Json
{
    /// <summary>
    /// The JSON that is sent to the site in response for requests to fetch or save the filter configuration.
    /// </summary>
    class FilterConfigurationJson : ResponseJson
    {
        /// <summary>
        /// Gets or sets a value that is incremented every time the settings are saved.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Gets or sets a newline separated list of aircraft ICAO codes.
        /// </summary>
        public string Icaos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Icaos"/> are ICAOs that are prohibited or the
        /// only ICAOs allowed.
        /// </summary>
        public bool ProhibitIcaos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that MLAT positions must not be allowed through the filter.
        /// </summary>
        public bool ProhibitMlat { get; set; }

        /// <summary>
        /// Creates a JSON object from a <see cref="FilterConfiguration"/> object.
        /// </summary>
        /// <param name="filterConfiguration"></param>
        /// <returns></returns>
        public void FromFilterConfiguration(FilterConfiguration filterConfiguration)
        {
            DataVersion =   filterConfiguration.DataVersion;
            ProhibitMlat =  filterConfiguration.ProhibitMlat;
            ProhibitIcaos = filterConfiguration.ProhibitIcaos;
            Icaos =         String.Join("\n", filterConfiguration.Icaos.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="FilterConfiguration"/> from the object.
        /// </summary>
        /// <param name="duplicateIcaos"></param>
        /// <param name="invalidIcaos"></param>
        /// <returns></returns>
        public FilterConfiguration ToFilterConfiguration(List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var result = new FilterConfiguration() {
                DataVersion =   DataVersion,
                ProhibitMlat =  ProhibitMlat,
                ProhibitIcaos = ProhibitIcaos,
            };
            result.Icaos.AddRange(ParseIcaos(Icaos, duplicateIcaos, invalidIcaos));

            return result;
        }

        /// <summary>
        /// Parses a single string containing multiple ICAOs into a collection of prohibited ICAOs.
        /// </summary>
        /// <param name="rawIcaos"></param>
        /// <param name="duplicateIcaos"></param>
        /// <param name="invalidIcaos"></param>
        /// <returns></returns>
        private string[] ParseIcaos(string rawIcaos, List<string> duplicateIcaos, List<string> invalidIcaos)
        {
            var result = new HashSet<string>();

            if(!String.IsNullOrEmpty(rawIcaos)) {
                foreach(var chunk in rawIcaos.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
                    var candidate = chunk.ToUpper().Trim();
                    if(candidate.Length < 6) candidate = String.Format("{0}{1}", new String('0', 6 - candidate.Length), candidate);
                    if(result.Contains(candidate)) {
                        if(!duplicateIcaos.Contains(candidate)) duplicateIcaos.Add(candidate);
                    } else if(candidate.Length > 6 || candidate.Any(r => !((r >= 'A' && r <= 'F') || (r >= '0' && r <= '9')))) {
                        if(!invalidIcaos.Contains(candidate)) invalidIcaos.Add(candidate);
                    } else {
                        result.Add(candidate);
                    }
                }
            }
            duplicateIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));
            invalidIcaos.Sort((lhs, rhs) => String.Compare(lhs, rhs));

            return result.OrderBy(r => r).ToArray();
        }
    }
}
