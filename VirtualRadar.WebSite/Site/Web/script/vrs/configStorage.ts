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

namespace VRS
{
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.configSuppressEraseOldSiteConfig = VRS.globalOptions.configSuppressEraseOldSiteConfig !== undefined ? VRS.globalOptions.configSuppressEraseOldSiteConfig : true; // True if the old site's configuration is not to be erased by the new site. If you set this to false it could lead the new site to be slighty less efficient when sending requests to the server.

    /**
     * Describes the options that can be passed to the import setting methods on ConfigStorage.
     */
    export interface ConfigStorage_ImportOptions
    {
        overwrite?:             boolean;
        resetBeforeImport?:     boolean;
        ignoreLanguage?:        boolean;
        ignoreSplitters?:       boolean;
        ignoreCurrentLocation?: boolean;
        ignoreAutoSelect?:      boolean;
        ignoreRequestFeedId?:   boolean;
    }

    /**
     * Handles the loading and saving of the site's configuration.
     */
    export class ConfigStorage
    {
        /**
         * The leading text for all VRS key names.
         */
        private _VRSKeyPrefix = 'VRadarServer-';

        /**
         * Gets or sets the prefix that distinguishes these options from other options stored for VRS by the browser.
         */
        private _Prefix = '';
        getPrefix() : string
        {
            return this._Prefix;
        }
        setPrefix(prefix: string)
        {
            this._Prefix = prefix;
        }

        /**
         * Gets the size in bytes of the configuration options stored by the browser.
         */
        getStorageSize() : number
        {
            return $.jStorage.storageSize();
        }

        /**
         * Gets a description of the storage engine used by this object to record configuration options.
         */
        getStorageEngine() : string
        {
            return 'jStorage-' + $.jStorage.currentBackend();
        }

        /**
         * Gets a value indicating that the user has saved settings. Ignores some settings that are auto-saved without
         * any user intervention.
         */
        getHasSettings() : boolean
        {
            var result = false;
            $.each(this.getAllVirtualRadarKeys(false), function(idx, keyName) {
                if(keyName !== 'VRadarServer-#global#-Localise') result = true;
                return !result;
            });

            return result;
        }

        /**
         * Checks that we have local storage available and displays an appropriate warning if we do not.
         */
        warnIfMissing()
        {
            if(!$.jStorage.currentBackend()) {
                VRS.pageHelper.showMessageBox(VRS.$$.Warning, VRS.$$.NoLocalStorage);
            }
        }

        /**
         * Loads a value from storage.
         */
        load(key: string, defaultValue?: any) : any
        {
            return this.doLoad(key, defaultValue, false);
        }

        /**
         * Loads a value from storage without using the page prefix. Only use for configuration settings that are
         * to be saved across every page on the site.
         */
        loadWithoutPrefix(key: string, defaultValue?: any) : any
        {
            return this.doLoad(key, defaultValue, true);
        };

        private doLoad(key: string, defaultValue?: any, ignorePrefix: boolean = false)
        {
            key = this.normaliseKey(key, ignorePrefix);
            return $.jStorage.get(key, defaultValue);
        }

        /**
         * Saves a value to storage.
         */
        save(key: string, value: any)
        {
            this.doSave(key, value, false);
        }

        /**
         * Saves a value to storage without using the prefix. Use for settings that need to persist across the entire site.
         */
        saveWithoutPrefix(key: string, value: any)
        {
            this.doSave(key, value, true);
        }

        private doSave(key: string, value: any, ignorePrefix: boolean)
        {
            key = this.normaliseKey(key, ignorePrefix);
            $.jStorage.set(key, value);
        }

        /**
         * Deletes a value from storage.
         */
        erase(key: string, ignorePrefix: boolean)
        {
            key = this.normaliseKey(key, ignorePrefix);
            $.jStorage.deleteKey(key);
        };

        /**
         * Removes configuration values stored by previous versions of VRS.
         */
        cleanupOldStorage()
        {
            if(!VRS.globalOptions.configSuppressEraseOldSiteConfig) {
                // Remove the cookies used by later versions of VRS.
                this.eraseCookie('googleMapOptions');
                this.eraseCookie('googleMapHomePin');
                this.eraseCookie('flightSimOptions');
                this.eraseCookie('reportMapOptions');

                // Way way WAY back VRS used to store lots of cookies - zap all of those, just in case
                this.eraseCookie('gmOptTraceType');
                this.eraseCookie('gmOptMapLatitude');
                this.eraseCookie('gmOptMapLongitude');
                this.eraseCookie('gmOptMapType');
                this.eraseCookie('gmOptMapZoom');
                this.eraseCookie('gmOptAutoDeselect');
                this.eraseCookie('gmOptAutoSelectClosest');
                this.eraseCookie('gmOptRefreshSeconds');
                this.eraseCookie('gmOptDisanceInKm');
                this.eraseCookie('gmOptShowOutlines');
                this.eraseCookie('gmOptPinTextLines');
                this.eraseCookie('gmOptcallOutSelected');
                this.eraseCookie('gmOptcallOutSelectedVol');
            }
        }

        private eraseCookie(name)
        {
            var yesterday = new Date(new Date().getTime() + (-1 * 86400000));
            document.cookie = name + '=; path=/; expires=' + yesterday.toUTCString();
        }

        /**
         * Mangles the key to avoid clashes with other applications / instances of VRS objects.
         */
        normaliseKey(key: string, ignorePrefix: boolean)
        {
            if(!key) throw 'A storage key must be supplied';
            var result = this._VRSKeyPrefix;
            result += ignorePrefix ? '#global#-' : this._Prefix ? '#' + this._Prefix + '#-' : '';
            result += key;

            return result;
        }

        /**
         * Returns an array of every key for every Virtual Radar Server settings held by the browser.
         */
        getAllVirtualRadarKeys(stripVrsPrefix: boolean = true) : string[]
        {
            var result: string[] = [];

            var self = this;
            var vrsKeyPrefixLength = this._VRSKeyPrefix.length;
            $.each($.jStorage.index(), function(idx, key) {
                if(VRS.stringUtility.startsWith(key, self._VRSKeyPrefix)) {
                    var keyName = stripVrsPrefix ? key.substr(vrsKeyPrefixLength) : this;
                    result.push(String(keyName));
                }
            });

            return result;
        }

        /**
         * Returns the value of a setting stored by the browser using the full key passed across, i.e. without adding
         * the user-configured prefix. Note that the global VRS prefix is always added to the key.
         */
        getContentWithoutPrefix(key: string) : any
        {
            return $.jStorage.get(this._VRSKeyPrefix + key, null);
        }

        /**
         * Deletes the value stored at the key without adding the user-configured prefix to the key. Note that the
         * global VRS prefix is always added to the key.
         */
        removeContentWithoutPrefix(key: string)
        {
            $.jStorage.deleteKey(this._VRSKeyPrefix + key);
        }

        /**
         * Deletes all stored values.
         */
        removeAllContent()
        {
            var self = this;
            $.each(this.getAllVirtualRadarKeys(), function() {
                self.removeContentWithoutPrefix(this);
            });
        }

        /**
         * Serialises the settings into a string and returns that string.
         */
        exportSettings()
        {
            var keys = this.getAllVirtualRadarKeys(false);
            var settings = { ver: 1, values: {} };
            $.each(keys, function(idx, key) {
                var obj = $.jStorage.get(key, null);
                if(obj !== null) settings.values[key] = obj;
            });

            var json = $.toJSON(settings);
            return json;
        };

        /**
         * Takes a serialised settings string, as created by exportSettings, and deserialises it. Overwrites the
         * current configuration with the deserialised values.
         */
        importSettings(exportString: string, options: ConfigStorage_ImportOptions)
        {
            options = $.extend({}, <ConfigStorage_ImportOptions> {
                overwrite:              true,
                resetBeforeImport:      false,
                ignoreLanguage:         false,
                ignoreSplitters:        false,
                ignoreCurrentLocation:  false,
                ignoreAutoSelect:       false,
                ignoreRequestFeedId:    false
            }, options);

            if(exportString) {
                var settings = $.parseJSON(exportString);
                if(settings && settings.ver) {
                    switch(settings.ver) {
                        case 1:
                            if(options.resetBeforeImport) {
                                this.removeAllContent();
                            }

                            for(var keyName in settings.values) {
                                if(settings.values.hasOwnProperty(keyName)) {
                                    if(this.isValidImportKey(keyName, options)) {
                                        var value = settings.values[keyName];
                                        this.adjustImportValues(keyName, value, options);
                                        if(value) {
                                            $.jStorage.set(keyName, value);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            throw 'These settings were exported from a later version of VRS. They cannot be loaded.';
                    }
                }
            }
        }

        /**
         * Returns true if the key name passed across represents a valid import key.
         */
        private isValidImportKey(keyName: string, options: ConfigStorage_ImportOptions) : boolean
        {
            var result = false;

            if(keyName && VRS.stringUtility.startsWith(keyName, this._VRSKeyPrefix)) {
                if(options.overwrite || $.jStorage.get(keyName, null) === null) {
                    result = true;

                    if(result && options.ignoreLanguage && keyName === 'VRadarServer-#global#-Localise') result = false;
                    if(result && options.ignoreSplitters && VRS.stringUtility.contains(keyName, '#-vrsSplitterPosition-')) result = false;
                    if(result && options.ignoreCurrentLocation && VRS.stringUtility.contains(keyName, '#-vrsCurrentLocation-')) result = false;
                    if(result && options.ignoreAutoSelect && VRS.stringUtility.contains(keyName, '#-vrsAircraftAutoSelect-')) result = false;
                }
            }

            return result;
        }

        /**
         * Modifies an imported value to comply with the options passed across.
         */
        private adjustImportValues(keyName: string, value: any, options: ConfigStorage_ImportOptions)
        {
            if(options.ignoreRequestFeedId) {
                var isVrsAircraftListFetcherValue = VRS.stringUtility.contains(keyName, '#-vrsAircraftListFetcher-');
                if(isVrsAircraftListFetcherValue) {
                    if(value['requestFeedId'] !== undefined) delete value['requestFeedId'];
                }
            }
        }
    }

    /**
     * The pre-built singleton configuration storage object.
     */
    export var configStorage: ConfigStorage = VRS.configStorage || new VRS.ConfigStorage();
}
