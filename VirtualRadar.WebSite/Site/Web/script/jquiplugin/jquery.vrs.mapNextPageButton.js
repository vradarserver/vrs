var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapNextPageButtonClass = VRS.globalOptions.mapNextPageButtonClass || 'mapNextPageButton';
    VRS.globalOptions.mapNextPageButtonImage = VRS.globalOptions.mapNextPageButtonImage || 'images/ChevronGreenCircle.png';
    VRS.globalOptions.mapNextPageButtonSize = VRS.globalOptions.mapNextPageButtonSize || { width: 26, height: 26 };
    VRS.globalOptions.mapNextPageButtonPausedImage = VRS.globalOptions.mapNextPageButtonPausedImage || 'images/ChevronRedCircle.png';
    VRS.globalOptions.mapNextPageButtonPausedSize = VRS.globalOptions.mapNextPageButtonPausedSize || { width: 26, height: 26 };
    VRS.globalOptions.mapNextPageButtonFilteredImage = VRS.globalOptions.mapNextPageButtonFilteredImage || 'images/ChevronBlueCircle.png';
    VRS.globalOptions.mapNextPageButtonFilteredSize = VRS.globalOptions.mapNextPageButtonFilteredSize || { width: 26, height: 26 };
    var MapNextPageButton_State = (function () {
        function MapNextPageButton_State() {
            this.imageElement = null;
            this.filterEnabledChangedHookResult = null;
            this.pausedChangedHookResult = null;
        }
        return MapNextPageButton_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMapNextPageButtonPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsMapNextPageButton');
    };
    VRS.jQueryUIHelper.getMapNextPageButtonOptions = function (overrides) {
        return $.extend({
            nextPageName: null,
            aircraftListFilter: null,
            aircraftListFetcher: null
        }, overrides);
    };
    var MapNextPageButton = (function (_super) {
        __extends(MapNextPageButton, _super);
        function MapNextPageButton() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getMapNextPageButtonOptions();
        }
        MapNextPageButton.prototype._getState = function () {
            var result = this.element.data('vrsMapNextPageButtonState');
            if (result === undefined) {
                result = new MapNextPageButton_State();
                this.element.data('vrsMapNextPageButtonState', result);
            }
            return result;
        };
        MapNextPageButton.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.mapNextPageButtonClass);
            state.imageElement = $('<img/>')
                .attr('src', VRS.globalOptions.mapNextPageButtonImage)
                .attr('width', VRS.globalOptions.mapNextPageButtonSize.width)
                .attr('height', VRS.globalOptions.mapNextPageButtonSize.height)
                .on('click', $.proxy(this._buttonClicked, this))
                .appendTo(this.element);
            if (options.aircraftListFetcher) {
                state.pausedChangedHookResult = options.aircraftListFetcher.hookPausedChanged(this._pausedChanged, this);
            }
            if (options.aircraftListFilter) {
                state.filterEnabledChangedHookResult = options.aircraftListFilter.hookEnabledChanged(this._filterEnabledChanged, this);
            }
            this._showImage();
        };
        MapNextPageButton.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.pausedChangedHookResult) {
                options.aircraftListFetcher.unhook(state.pausedChangedHookResult);
                state.pausedChangedHookResult = null;
            }
            if (state.filterEnabledChangedHookResult) {
                options.aircraftListFilter.unhook(state.filterEnabledChangedHookResult);
                state.filterEnabledChangedHookResult = null;
            }
            if (state.imageElement) {
                state.imageElement.off();
                state.imageElement.remove();
                state.imageElement = null;
            }
            this.element.removeClass(VRS.globalOptions.mapNextPageButtonClass);
        };
        MapNextPageButton.prototype._showImage = function () {
            var state = this._getState();
            var options = this.options;
            var image = VRS.globalOptions.mapNextPageButtonImage;
            var size = VRS.globalOptions.mapNextPageButtonSize;
            if (options.aircraftListFetcher && options.aircraftListFetcher.getPaused()) {
                image = VRS.globalOptions.mapNextPageButtonPausedImage;
                size = VRS.globalOptions.mapNextPageButtonPausedSize;
            }
            else if (options.aircraftListFilter && options.aircraftListFilter.getEnabled()) {
                image = VRS.globalOptions.mapNextPageButtonFilteredImage;
                size = VRS.globalOptions.mapNextPageButtonFilteredSize;
            }
            state.imageElement.prop('width', size.width);
            state.imageElement.prop('height', size.height);
            state.imageElement.prop('src', image);
        };
        MapNextPageButton.prototype._buttonClicked = function (event) {
            VRS.pageManager.show(this.options.nextPageName);
            event.stopPropagation();
            return false;
        };
        MapNextPageButton.prototype._pausedChanged = function () {
            this._showImage();
        };
        MapNextPageButton.prototype._filterEnabledChanged = function () {
            this._showImage();
        };
        return MapNextPageButton;
    }(JQueryUICustomWidget));
    VRS.MapNextPageButton = MapNextPageButton;
    $.widget('vrs.vrsMapNextPageButton', new MapNextPageButton());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.mapNextPageButton.js.map