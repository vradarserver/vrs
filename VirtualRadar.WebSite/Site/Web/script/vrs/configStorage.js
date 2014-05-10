/**
 * @license Copyright © 2013 onwards, Andrew Whewell
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
 * @fileoverview Handles the storage of configuration options on the browser side.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.configSuppressEraseOldSiteConfig = VRS.globalOptions.configSuppressEraseOldSiteConfig !== undefined ? VRS.globalOptions.configSuppressEraseOldSiteConfig : true; // True if the old site's configuration is not to be erased by the new site. If you set this to false it could lead the new site to be slighty less efficient when sending requests to the server.
    //endregion

    //region ConfigStorage
    /**
     * Handles the loading and saving of the site's configuration.
     * @constructor
     */
    VRS.ConfigStorage = function()
    {
        /**
         * The leading text for all VRS key names.
         * @type {string}
         * @private
         */
        var _VRSKeyPrefix = 'VRadarServer-';

        //region -- Properties
        var _Prefix = '';
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the prefix that distinguishes these options from other options stored for VRS by the browser.
         * @returns {string}
         */
        this.getPrefix = function()
        {
            return _Prefix;
        };
        /**
         * Sets the prefix that distinguishes these options from other options stored for VRS by the browser.
         * @param {string} prefix
         */
        this.setPrefix = function(prefix)
        {
            _Prefix = prefix;
        };

        /**
         * Returns the size in bytes of the configuration options stored by the browser.
         * @returns {Number}
         */
        this.getStorageSize = function()
        {
            return $.jStorage.storageSize();
        };

        /**
         * Gets a description of the storage engine used by this object to record configuration options.
         * @returns {string}
         */
        this.getStorageEngine = function()
        {
            return 'jStorage-' + $.jStorage.currentBackend();
        };
        //endregion

        //region -- warnIfMissing
        /**
         * Checks that we have local storage available and displays an appropriate warning if we do not.
         */
        this.warnIfMissing = function()
        {
            if(!$.jStorage.currentBackend()) {
                VRS.pageHelper.showMessageBox(VRS.$$.Warning, VRS.$$.NoLocalStorage);
            }
        };
        //endregion

        //region -- load, save, erase
        /**
         * Loads a value from storage.
         * @param {string}   key            The unique identifier of the value.
         * @param {*}       [defaultValue]  The value to return if the key does not exist. Note that if this is undefined then null is returned.
         * @returns {*}
         */
        this.load = function(key, defaultValue)
        {
            return doLoad(key, defaultValue, false);
        };

        /**
         * Loads a value from storage without using the page prefix. Only use for configuration settings that are
         * to be saved across every page on the site.
         * @param {string}      key
         * @param {*}           defaultValue
         * @returns {*}
         */
        this.loadWithoutPrefix = function(key, defaultValue)
        {
            return doLoad(key, defaultValue, true);
        };

        /**
         * Does the work for the load functions.
         * @param {string}      key
         * @param {*}           defaultValue
         * @param {bool}        ignorePrefix
         * @returns {*}
         */
        function doLoad(key, defaultValue, ignorePrefix)
        {
            key = normaliseKey(key, ignorePrefix);
            return $.jStorage.get(key, defaultValue);
        }

        /**
         * Saves a value to storage.
         * @param {string}  key     The unique identifier of the value.
         * @param {*}       value   The value to save.
         */
        this.save = function(key, value)
        {
            doSave(key, value, false);
        };

        /**
         * Saves a value to storage without using the prefix. Use for settings that need to persist across the entire site.
         * @param {string}  key     The unique identifier of the value.
         * @param {*}       value   The value to save.
         */
        this.saveWithoutPrefix = function(key, value)
        {
            doSave(key, value, true);
        };

        /**
         * Does the work for the save functions.
         * @param {string}  key
         * @param {*}       value
         * @param {bool}    ignorePrefix
         */
        function doSave(key, value, ignorePrefix)
        {
            key = normaliseKey(key, ignorePrefix);
            $.jStorage.set(key, value);
        }

        //noinspection JSUnusedGlobalSymbols
        /**
         * Deletes a value from storage.
         * @param {string} key              The unique identifier of the value to delete.
         * @param {bool}   ignorePrefix     True if the setting ignores the prefix, false if it doesn't.
         */
        this.erase = function(key, ignorePrefix)
        {
            key = normaliseKey(key, ignorePrefix);
            $.jStorage.deleteKey(key);
        };
        //endregion

        //region -- cleanupOldStorage
        /**
         * Removes configuration values stored by previous versions of VRS.
         */
        this.cleanupOldStorage = function()
        {
            if(!VRS.globalOptions.configSuppressEraseOldSiteConfig) {
                // Remove the cookies used by later versions of VRS.
                eraseCookie('googleMapOptions');
                eraseCookie('googleMapHomePin');
                eraseCookie('flightSimOptions');
                eraseCookie('reportMapOptions');

                // Way way WAY back VRS used to store lots of cookies - zap all of those, just in case
                eraseCookie('gmOptTraceType');
                eraseCookie('gmOptMapLatitude');
                eraseCookie('gmOptMapLongitude');
                eraseCookie('gmOptMapType');
                eraseCookie('gmOptMapZoom');
                eraseCookie('gmOptAutoDeselect');
                eraseCookie('gmOptAutoSelectClosest');
                eraseCookie('gmOptRefreshSeconds');
                eraseCookie('gmOptDisanceInKm');
                eraseCookie('gmOptShowOutlines');
                eraseCookie('gmOptPinTextLines');
                eraseCookie('gmOptcallOutSelected');
                eraseCookie('gmOptcallOutSelectedVol');
            }
        };

        /**
         * Erases a VRS cookie.
         * @param name
         */
        function eraseCookie(name)
        {
            var yesterday = new Date(new Date().getTime() + (-1 * 86400000));
            document.cookie = name + '=; path=/; expires=' + yesterday.toUTCString();
        }
        //endregion

        //region -- normaliseKey
        /**
         * Mangles the key to avoid clashes with other applications / instances of VRS objects.
         * @param {string}          key             The unique ID originally assigned to a value.
         * @param {boolean}        [ignorePrefix]   True if the prefix should not be applied.
         * @returns {string} The new unique ID with the global VRS prefix and the user-specified prefix added.
         */
        function normaliseKey(key, ignorePrefix)
        {
            if(!key) throw 'A storage key must be supplied';
            var result = _VRSKeyPrefix;
            result += ignorePrefix ? '#global#-' : _Prefix ? '#' + _Prefix + '#-' : '';
            result += key;

            return result;
        }
        //endregion

        //region -- getAllVirtualRadarKeys, getContentWithoutPrefix, removeContentWithoutPrefix
        /**
         * Returns an array of every key for every Virtual Radar Server settings held by the browser.
         * @param {bool} [stripVrsPrefix]   True if the key names should have the VRS prefix removed from them. Note that this does NOT remove the user-specified prefix.
         * @returns {string[]} The array of key names.
         */
        this.getAllVirtualRadarKeys = function(stripVrsPrefix)
        {
            if(stripVrsPrefix === undefined) stripVrsPrefix = true;

            var result = [];

            var vrsKeyPrefixLength = _VRSKeyPrefix.length;
            $.each($.jStorage.index(), function(idx, key) {
                if(VRS.stringUtility.startsWith(key, _VRSKeyPrefix)) {
                    var keyName = stripVrsPrefix ? key.substr(vrsKeyPrefixLength) : this;
                    result.push(String(keyName));
                }
            });

            return result;
        };

        /**
         * Returns the value of a setting stored by the browser using the full key passed across, i.e. without adding
         * the user-configured prefix. Note that the global VRS prefix is always added to the key.
         * @param {string} key
         * @returns {*}
         */
        this.getContentWithoutPrefix = function(key)
        {
            return $.jStorage.get(_VRSKeyPrefix + key, null);
        };

        /**
         * Deletes the value stored at the key without adding the user-configured prefix to the key. Note that the
         * global VRS prefix is always added to the key.
         * @param key
         */
        this.removeContentWithoutPrefix = function(key)
        {
            $.jStorage.deleteKey(_VRSKeyPrefix + key);
        };
        //endregion
    };
    //endregion

    //region Pre-builts
    /**
     * The pre-built singleton configuration storage object.
     * @type {VRS.ConfigStorage}
     */
    VRS.configStorage = VRS.configStorage || new VRS.ConfigStorage();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));