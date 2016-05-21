var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var AircraftLinksPlugin_State = (function () {
        function AircraftLinksPlugin_State() {
            this.aircraft = undefined;
            this.linkElements = [];
            this.linkVisible = [];
            this.separatorElements = [];
        }
        return AircraftLinksPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftLinksPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsAircraftLinks');
    };
    VRS.jQueryUIHelper.getAircraftLinksOptions = function (overrides) {
        return $.extend({
            linkSites: null
        }, overrides);
    };
    var AircraftLinksPlugin = (function (_super) {
        __extends(AircraftLinksPlugin, _super);
        function AircraftLinksPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getAircraftLinksOptions();
        }
        AircraftLinksPlugin.prototype._getState = function () {
            var result = this.element.data('aircraftLinksPluginState');
            if (result === undefined) {
                result = new AircraftLinksPlugin_State();
                this.element.data('aircraftLinksPluginState', result);
            }
            return result;
        };
        AircraftLinksPlugin.prototype._create = function () {
            var state = this._getState();
            if (!this.options.linkSites) {
                this.options.linkSites = VRS.linksRenderer.getDefaultAircraftLinkSites();
            }
            this.element.addClass('aircraftLinks');
        };
        AircraftLinksPlugin.prototype._destroy = function () {
            var state = this._getState();
            this._removeLinkElements(state);
            state.aircraft = undefined;
        };
        AircraftLinksPlugin.prototype.renderForAircraft = function (aircraft, forceRefresh) {
            var state = this._getState();
            if (state.aircraft !== aircraft) {
                forceRefresh = true;
            }
            state.aircraft = aircraft;
            this.doReRender(forceRefresh, state);
        };
        AircraftLinksPlugin.prototype.reRender = function (forceRefresh) {
            this.doReRender(forceRefresh, this._getState());
        };
        AircraftLinksPlugin.prototype.doReRender = function (forceRefresh, state) {
            var _this = this;
            if (!state)
                state = this._getState();
            var options = this.options;
            var aircraft = state.aircraft;
            if (options.linkSites.length < 1) {
                this._removeLinkElements(state);
            }
            else {
                if (state.linkElements.length !== options.linkSites.length) {
                    this._removeLinkElements(state);
                    $.each(options.linkSites, function (idx, siteOrHandler) {
                        var handler = VRS.linksRenderer.findLinkHandler(siteOrHandler);
                        if (idx !== 0) {
                            state.separatorElements.push($('<span/>')
                                .text(VRS.globalOptions.linkSeparator)
                                .hide()
                                .appendTo(_this.element));
                        }
                        var linkElement = $('<a/>')
                            .attr('href', '#')
                            .attr('target', '_self')
                            .addClass(VRS.globalOptions.linkClass)
                            .text('')
                            .hide()
                            .appendTo(_this.element);
                        if (handler && handler.onClick) {
                            linkElement.on('click', function (event) {
                                if (VRS.timeoutManager) {
                                    VRS.timeoutManager.resetTimer();
                                }
                                handler.onClick(event);
                            });
                        }
                        state.linkElements.push(linkElement);
                        state.linkVisible.push(false);
                    });
                }
                $.each(options.linkSites, function (idx, linkSite) {
                    var linkElement = state.linkElements[idx];
                    var handler = VRS.linksRenderer.findLinkHandler(linkSite);
                    var canShowLink = !!(handler && handler.canLinkAircraft(aircraft));
                    if (!canShowLink) {
                        linkElement.hide();
                        state.linkVisible[idx] = false;
                    }
                    else {
                        if (forceRefresh || !state.linkVisible[idx] || handler.hasChanged(aircraft)) {
                            linkElement
                                .attr('href', handler.buildUrl(aircraft))
                                .attr('target', handler.getTarget(aircraft))
                                .text(handler.getTitle(aircraft))
                                .show();
                            state.linkVisible[idx] = true;
                        }
                    }
                });
                var atLeastOneLinkVisible = false;
                $.each(state.linkVisible, function (idx, linkVisible) {
                    if (idx > 0) {
                        var showSeparator = linkVisible && atLeastOneLinkVisible;
                        var separatorElement = state.separatorElements[idx - 1];
                        if (showSeparator)
                            separatorElement.show();
                        else
                            separatorElement.hide();
                    }
                    if (linkVisible)
                        atLeastOneLinkVisible = true;
                });
            }
        };
        AircraftLinksPlugin.prototype._removeLinkElements = function (state) {
            $.each(state.linkElements, function (idx, element) {
                if (element && element instanceof jQuery) {
                    element.off();
                    element.remove();
                }
            });
            state.linkElements = [];
            state.linkVisible = [];
            $.each(state.separatorElements, function (idx, element) {
                element.remove();
            });
            state.separatorElements = [];
        };
        return AircraftLinksPlugin;
    }(JQueryUICustomWidget));
    VRS.AircraftLinksPlugin = AircraftLinksPlugin;
    $.widget('vrs.vrsAircraftLinks', new AircraftLinksPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.aircraftLinks.js.map