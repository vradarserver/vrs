var VRS;
(function (VRS) {
    VRS.globalEvent = $.extend(VRS.globalEvent || {}, {
        bootstrapCreated: 'bootstrapCreated',
        displayUpdated: 'displayUpdated',
        serverConfigChanged: 'serverConfigChanged'
    });
    var EventHandler = (function () {
        function EventHandler(settings) {
            this._Events = {};
            this._Settings = $.extend({
                name: 'anon',
                logLevel: 0
            }, settings);
        }
        EventHandler.prototype.hook = function (eventName, callback, forceThis) {
            if (forceThis === void 0) { forceThis = window; }
            if (!this._Events[eventName])
                this._Events[eventName] = [];
            this._Events[eventName].push({ callback: callback, forceThis: forceThis });
            return { eventName: eventName, callback: callback };
        };
        EventHandler.prototype.unhook = function (hookResult) {
            if (hookResult) {
                var listeners = this._Events[hookResult.eventName];
                var self = this;
                if (listeners)
                    $.each(listeners, function (idx, handle) {
                        if (handle.callback === hookResult.callback) {
                            self._Events[hookResult.eventName].splice(idx, 1);
                            return false;
                        }
                        return true;
                    });
            }
        };
        EventHandler.prototype.raise = function (eventName, args) {
            var startTime = this._Settings.logLevel ? new Date() : null;
            var listeners = this._Events[eventName];
            if (listeners) {
                var length = listeners.length;
                for (var i = 0; i < length; ++i) {
                    var handler = listeners[i];
                    if (handler) {
                        handler.callback.apply(handler.forceThis, args || []);
                    }
                }
            }
            if (startTime) {
                var elapsed = new Date().getTime() - startTime.getTime();
                console.log('EVT ' + this._Settings.name + '.' + eventName + ' took ' + elapsed + 'ms with ' + (listeners ? listeners.length : 0) + ' listeners');
            }
        };
        EventHandler.prototype.hookJQueryUIPluginEvent = function (pluginElement, pluginName, eventName, callback, forceThis) {
            if (forceThis === void 0) { forceThis = null; }
            var fullEventName = (pluginName + eventName).toLowerCase();
            if (forceThis)
                callback = $.proxy(callback, forceThis);
            pluginElement.bind(fullEventName, callback);
            return { eventName: fullEventName };
        };
        EventHandler.prototype.unhookJQueryUIPluginEvent = function (pluginElement, hookResult) {
            if (pluginElement && hookResult && hookResult.eventName) {
                pluginElement.unbind(hookResult.eventName);
            }
        };
        return EventHandler;
    })();
    VRS.EventHandler = EventHandler;
    VRS.globalDispatch = new VRS.EventHandler({
        name: 'GlobalDispatch'
    });
})(VRS || (VRS = {}));
//# sourceMappingURL=event.js.map