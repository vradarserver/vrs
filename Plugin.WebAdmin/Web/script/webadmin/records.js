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
 * @fileoverview A class that can help with keeping track of previous versions of a collection of records sent from the server.
 */

(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};

    /**
     * Describes a collection of records sent from the server.
     * @param {VRS_WEBADMIN_RECORDCOLLECTION_SETTINGS} settings
     * @constructor
     */
    VRS.WebAdmin.RecordCollection = function(settings)
    {
        var that = this;

        var _PreviousValues = [];
        var _CurrentValues = [];

        /**
         * Stores new versions of the records.
         * @param {object[]} records
         */
        this.applyUpdates = function(records)
        {
            _PreviousValues = _CurrentValues;
            _CurrentValues = records || [];
        };

        /**
         * Returns the current records.
         * @returns {Array}
         */
        this.getCurrentRecords = function()
        {
            return _CurrentValues;
        };

        /**
         * Returns the previous versions of the records.
         * @returns {Array}
         */
        this.getPreviousRecords = function()
        {
            return _PreviousValues;
        };

        /**
         * Returns the identifier for the record passed across.
         * @param {object} record
         * @returns {*}
         */
        this.getRecordId = function(record)
        {
            return settings.getId(record);
        };

        /**
         * Returns the current record that matches the identifier passed across.
         * @param {*} id
         * @returns {object|null}
         */
        this.getCurrentRecordById = function(id)
        {
            return recordById(_CurrentValues, id);
        };

        /**
         * Returns the previous record that matches the identifier passed across.
         * @param {*} id
         * @returns {Object|null}
         */
        this.getPreviousRecordById = function(id)
        {
            return recordById(_PreviousValues, id);
        };

        /**
         * Returns a record from an array of records that matches the ID passed across.
         * @param {object[]}    records
         * @param {*}           id
         * @returns {object|null}
         */
        function recordById(records, id)
        {
            var result = null;
            var length = records.length;
            for(var i = 0;i < length;++i) {
                var record = records[i];
                var recordId = settings.getId(record);
                if(recordId === id) {
                    result = record;
                    break;
                }
            }

            return result;
        }
    };
}(window.VRS = window.VRS || {}, jQuery));