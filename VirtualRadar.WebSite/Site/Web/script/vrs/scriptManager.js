var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.scriptManagerTimeout = VRS.globalOptions.scriptManagerTimeout || 30000;
    VRS.scriptKey = {
        GoogleMaps: 'googleMap'
    };
    var ScriptManager = (function () {
        function ScriptManager() {
            this._LoadedScripts = {};
            this._Queue = [];
        }
        ScriptManager.prototype.loadScript = function (options) {
            options = $.extend({
                key: null,
                params: {},
                queue: false,
                success: $.noop,
                error: null,
                timeout: VRS.globalOptions.scriptManagerTimeout
            }, options);
            if (!options.queue) {
                this.doLoadScript(options);
            }
            else {
                this._Queue.push(options);
                if (this._Queue.length === 1)
                    this.doProcessQueue();
            }
        };
        ScriptManager.prototype.doProcessQueue = function () {
            var self = this;
            if (this._Queue.length) {
                var queueEntry = this._Queue[0];
                this.doLoadScript(queueEntry, function () {
                    self._Queue.splice(0, 1);
                    self.doProcessQueue();
                });
            }
        };
        ScriptManager.prototype.doLoadScript = function (options, onCompletion) {
            var self = this;
            var callSuccess = function () {
                options.success();
                if (onCompletion)
                    onCompletion();
            };
            if (options.key && this._LoadedScripts[options.key]) {
                callSuccess();
            }
            else {
                if (options.key !== VRS.scriptKey.GoogleMaps) {
                    $.ajax({
                        url: options.url,
                        data: options.params,
                        success: function () {
                            if (options.key)
                                self._LoadedScripts[options.key] = true;
                            callSuccess();
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            if (!options.error)
                                throw 'Could not load the script "' + (options.key || '') + '" from "' + options.url + '". Status: ' + (textStatus || '') + '. Error: ' + (errorThrown || '');
                            options.error(jqXHR, textStatus, errorThrown);
                            if (onCompletion)
                                onCompletion();
                        },
                        dataType: 'script',
                        timeout: VRS.globalOptions.scriptManagerTimeout
                    });
                }
                else {
                    var callbackName = 'googleMapCallback_' + $.now();
                    options.params = $.extend({}, options.params, { callback: callbackName });
                    var timeoutId = setTimeout(function () {
                        timeoutId = 0;
                        VRS.pageHelper.showMessageBox('Google Maps', 'Could not load Google Maps within ' + options.timeout + 'ms');
                        if (options.error) {
                            options.error(null, 'Timed out', 'Timed out');
                            if (onCompletion)
                                onCompletion();
                        }
                    }, options.timeout);
                    window[callbackName] = function () {
                        if (timeoutId)
                            clearTimeout(timeoutId);
                        callSuccess();
                    };
                    var fullUrl = options.url + '?' + $.param(options.params);
                    var script = $('<script/>')
                        .attr('type', 'text/javascript')
                        .attr('src', fullUrl);
                    $(document).find('head').last().append(script);
                }
            }
        };
        return ScriptManager;
    })();
    VRS.ScriptManager = ScriptManager;
    VRS.scriptManager = new VRS.ScriptManager();
})(VRS || (VRS = {}));
//# sourceMappingURL=scriptManager.js.map