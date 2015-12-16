/**
 * @license Copyright © 2014 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview Handles calls to www.airport-data.com's API.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.airportDataApiThumbnailsUrl = VRS.globalOptions.airportDataApiThumbnailsUrl || 'AirportDataThumbnails.json';  // The URL for the airport-data API thumbnails call.
    VRS.globalOptions.airportDataApiTimeout = VRS.globalOptions.airportDataApiTimeout || 10000;                                     // The timeout, in milliseconds, for airport-data API calls.

    export class AirportDataApi
    {
        /**
         * Fetches thumbnail images from www.airport-data.com. The callback is passed the result once the fetch has
         * completed. The parameters to the callback are the ICAO and the result object. If the fetch fails then the
         * callback is passed a JSON object in an error state.
         */
        getThumbnails(icao: string, registration: string, countThumbnails: number, callback: (icao:string, data:IAirportDataThumbnails) => void)
        {
            $.ajax({
                url:        VRS.globalOptions.airportDataApiThumbnailsUrl,
                dataType:   'json',
                data:       { icao: icao, reg: registration, numThumbs: countThumbnails },
                error:      function(jqXHR, textStatus) {
                                callback(icao, { status: jqXHR.status, error: 'XHR call failed: ' + textStatus });
                            },
                success:    function(data: IAirportDataThumbnails) {
                                callback(icao, data);
                            },
                timeout:    VRS.globalOptions.airportDataApiTimeout
            });
        }
    }
}

 