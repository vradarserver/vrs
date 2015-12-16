var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.configSuppressEraseOldSiteConfig = VRS.globalOptions.configSuppressEraseOldSiteConfig !== undefined ? VRS.globalOptions.configSuppressEraseOldSiteConfig : true;
    var ConfigStorage = (function () {
        function ConfigStorage() {
            this._VRSKeyPrefix = 'VRadarServer-';
            this._Prefix = '';
        }
        ConfigStorage.prototype.getPrefix = function () {
            return this._Prefix;
        };
        ConfigStorage.prototype.setPrefix = function (prefix) {
            this._Prefix = prefix;
        };
        ConfigStorage.prototype.getStorageSize = function () {
            return $.jStorage.storageSize();
        };
        ConfigStorage.prototype.getStorageEngine = function () {
            return 'jStorage-' + $.jStorage.currentBackend();
        };
        ConfigStorage.prototype.getHasSettings = function () {
            var result = false;
            $.each(this.getAllVirtualRadarKeys(false), function (idx, keyName) {
                if (keyName !== 'VRadarServer-#global#-Localise')
                    result = true;
                return !result;
            });
            return result;
        };
        ConfigStorage.prototype.warnIfMissing = function () {
            if (!$.jStorage.currentBackend()) {
                VRS.pageHelper.showMessageBox(VRS.$$.Warning, VRS.$$.NoLocalStorage);
            }
        };
        ConfigStorage.prototype.load = function (key, defaultValue) {
            return this.doLoad(key, defaultValue, false);
        };
        ConfigStorage.prototype.loadWithoutPrefix = function (key, defaultValue) {
            return this.doLoad(key, defaultValue, true);
        };
        ;
        ConfigStorage.prototype.doLoad = function (key, defaultValue, ignorePrefix) {
            if (ignorePrefix === void 0) { ignorePrefix = false; }
            key = this.normaliseKey(key, ignorePrefix);
            return $.jStorage.get(key, defaultValue);
        };
        ConfigStorage.prototype.save = function (key, value) {
            this.doSave(key, value, false);
        };
        ConfigStorage.prototype.saveWithoutPrefix = function (key, value) {
            this.doSave(key, value, true);
        };
        ConfigStorage.prototype.doSave = function (key, value, ignorePrefix) {
            key = this.normaliseKey(key, ignorePrefix);
            $.jStorage.set(key, value);
        };
        ConfigStorage.prototype.erase = function (key, ignorePrefix) {
            key = this.normaliseKey(key, ignorePrefix);
            $.jStorage.deleteKey(key);
        };
        ;
        ConfigStorage.prototype.cleanupOldStorage = function () {
            if (!VRS.globalOptions.configSuppressEraseOldSiteConfig) {
                this.eraseCookie('googleMapOptions');
                this.eraseCookie('googleMapHomePin');
                this.eraseCookie('flightSimOptions');
                this.eraseCookie('reportMapOptions');
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
        };
        ConfigStorage.prototype.eraseCookie = function (name) {
            var yesterday = new Date(new Date().getTime() + (-1 * 86400000));
            document.cookie = name + '=; path=/; expires=' + yesterday.toUTCString();
        };
        ConfigStorage.prototype.normaliseKey = function (key, ignorePrefix) {
            if (!key)
                throw 'A storage key must be supplied';
            var result = this._VRSKeyPrefix;
            result += ignorePrefix ? '#global#-' : this._Prefix ? '#' + this._Prefix + '#-' : '';
            result += key;
            return result;
        };
        ConfigStorage.prototype.getAllVirtualRadarKeys = function (stripVrsPrefix) {
            if (stripVrsPrefix === void 0) { stripVrsPrefix = true; }
            var result = [];
            var self = this;
            var vrsKeyPrefixLength = this._VRSKeyPrefix.length;
            $.each($.jStorage.index(), function (idx, key) {
                if (VRS.stringUtility.startsWith(key, self._VRSKeyPrefix)) {
                    var keyName = stripVrsPrefix ? key.substr(vrsKeyPrefixLength) : this;
                    result.push(String(keyName));
                }
            });
            return result;
        };
        ConfigStorage.prototype.getContentWithoutPrefix = function (key) {
            return $.jStorage.get(this._VRSKeyPrefix + key, null);
        };
        ConfigStorage.prototype.removeContentWithoutPrefix = function (key) {
            $.jStorage.deleteKey(this._VRSKeyPrefix + key);
        };
        ConfigStorage.prototype.removeAllContent = function () {
            var self = this;
            $.each(this.getAllVirtualRadarKeys(), function () {
                self.removeContentWithoutPrefix(this);
            });
        };
        ConfigStorage.prototype.exportSettings = function () {
            var keys = this.getAllVirtualRadarKeys(false);
            var settings = { ver: 1, values: {} };
            $.each(keys, function (idx, key) {
                var obj = $.jStorage.get(key, null);
                if (obj !== null)
                    settings.values[key] = obj;
            });
            var json = $.toJSON(settings);
            return json;
        };
        ;
        ConfigStorage.prototype.importSettings = function (exportString, options) {
            options = $.extend({}, {
                overwrite: true,
                resetBeforeImport: false,
                ignoreLanguage: false,
                ignoreSplitters: false,
                ignoreCurrentLocation: false,
                ignoreAutoSelect: false,
                ignoreRequestFeedId: false
            }, options);
            if (exportString) {
                var settings = $.parseJSON(exportString);
                if (settings && settings.ver) {
                    switch (settings.ver) {
                        case 1:
                            if (options.resetBeforeImport) {
                                this.removeAllContent();
                            }
                            for (var keyName in settings.values) {
                                if (settings.values.hasOwnProperty(keyName)) {
                                    if (this.isValidImportKey(keyName, options)) {
                                        var value = settings.values[keyName];
                                        this.adjustImportValues(keyName, value, options);
                                        if (value) {
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
        };
        ConfigStorage.prototype.isValidImportKey = function (keyName, options) {
            var result = false;
            if (keyName && VRS.stringUtility.startsWith(keyName, this._VRSKeyPrefix)) {
                if (options.overwrite || $.jStorage.get(keyName, null) === null) {
                    result = true;
                    if (result && options.ignoreLanguage && keyName === 'VRadarServer-#global#-Localise')
                        result = false;
                    if (result && options.ignoreSplitters && VRS.stringUtility.contains(keyName, '#-vrsSplitterPosition-'))
                        result = false;
                    if (result && options.ignoreCurrentLocation && VRS.stringUtility.contains(keyName, '#-vrsCurrentLocation-'))
                        result = false;
                    if (result && options.ignoreAutoSelect && VRS.stringUtility.contains(keyName, '#-vrsAircraftAutoSelect-'))
                        result = false;
                }
            }
            return result;
        };
        ConfigStorage.prototype.adjustImportValues = function (keyName, value, options) {
            if (options.ignoreRequestFeedId) {
                var isVrsAircraftListFetcherValue = VRS.stringUtility.contains(keyName, '#-vrsAircraftListFetcher-');
                if (isVrsAircraftListFetcherValue) {
                    if (value['requestFeedId'] !== undefined)
                        delete value['requestFeedId'];
                }
            }
        };
        return ConfigStorage;
    })();
    VRS.ConfigStorage = ConfigStorage;
    VRS.configStorage = VRS.configStorage || new VRS.ConfigStorage();
})(VRS || (VRS = {}));
//# sourceMappingURL=configStorage.js.map