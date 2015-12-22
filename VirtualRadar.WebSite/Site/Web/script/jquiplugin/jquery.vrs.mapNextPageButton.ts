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
 * @fileoverview A jQuery UI plugin that displays a button that, when clicked, switches pages. Intended for use on mobile
 * maps.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapNextPageButtonClass = VRS.globalOptions.mapNextPageButtonClass || 'mapNextPageButton';                             // The class to use for the map next page button.
    VRS.globalOptions.mapNextPageButtonImage = VRS.globalOptions.mapNextPageButtonImage || 'images/ChevronGreenCircle.png';                 // The image for the map next page button.
    VRS.globalOptions.mapNextPageButtonSize = VRS.globalOptions.mapNextPageButtonSize || { width: 26, height: 26 };                         // The size of the image for the map next page button.
    VRS.globalOptions.mapNextPageButtonPausedImage = VRS.globalOptions.mapNextPageButtonPausedImage || 'images/ChevronRedCircle.png';       // The image for the map next page button when paused.
    VRS.globalOptions.mapNextPageButtonPausedSize = VRS.globalOptions.mapNextPageButtonPausedSize || { width: 26, height: 26 };             // The size of the image for the map next page button when paused.
    VRS.globalOptions.mapNextPageButtonFilteredImage = VRS.globalOptions.mapNextPageButtonFilteredImage || 'images/ChevronBlueCircle.png';  // The image for the map next page button when filtered.
    VRS.globalOptions.mapNextPageButtonFilteredSize = VRS.globalOptions.mapNextPageButtonFilteredSize || { width: 26, height: 26 };         // The size of the image for the map next page button when filtered.

    /**
     * The state object for the MapNextPageButton widget.
     */
    class MapNextPageButton_State
    {
        /**
         * The element that holds the image of the button.
         */
        imageElement: JQuery = null;

        /**
         * The hook for the filter enabled event.
         */
        filterEnabledChangedHookResult: IEventHandle = null;

        /**
         * The hook for the paused changed event.
         */
        pausedChangedHookResult: IEventHandle = null;
    }

    /**
     * The options honoured by the MapNextPageButton widget.
     */
    export interface MapNextPageButton_Options
    {
        /**
         * The name of the page to show when the button is pressed
         */
        nextPageName: string;

        /**
         * The aircraft list filter - if supplied, and if the updates are being filtered, this changes the button's image
         */
        aircraftListFilter?: AircraftListFilter;

        /**
         * If this fetcher is supplied and it indicates that the updates are paused then it changes the button's image
         */
        aircraftListFetcher?: AircraftListFetcher;
    }

    /*
     * JQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMapNextPageButtonPlugin = function(jQueryElement: JQuery)
    {
        return <MapNextPageButton>jQueryElement.data('vrsVrsMapNextPageButton');
    }
    VRS.jQueryUIHelper.getMapNextPageButtonOptions = function(overrides?: MapNextPageButton_Options) : MapNextPageButton_Options
    {
        return $.extend({
            nextPageName:   null,
            aircraftListFilter: null,
            aircraftListFetcher: null
        }, overrides);
    }

    /**
     * A widget that handles the next page button on the mobile map.
     */
    export class MapNextPageButton extends JQueryUICustomWidget
    {
        options: MapNextPageButton_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getMapNextPageButtonOptions();
        }

        private _getState() : MapNextPageButton_State
        {
            var result = this.element.data('vrsMapNextPageButtonState');
            if(result === undefined) {
                result = new MapNextPageButton_State();
                this.element.data('vrsMapNextPageButtonState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.mapNextPageButtonClass);

            state.imageElement = $('<img/>')
                .attr('src', VRS.globalOptions.mapNextPageButtonImage)
                .attr('width', VRS.globalOptions.mapNextPageButtonSize.width)
                .attr('height', VRS.globalOptions.mapNextPageButtonSize.height)
                .on('click', $.proxy(this._buttonClicked, this))
                .appendTo(this.element);

            if(options.aircraftListFetcher) {
                state.pausedChangedHookResult = options.aircraftListFetcher.hookPausedChanged(this._pausedChanged, this);
            }
            if(options.aircraftListFilter) {
                state.filterEnabledChangedHookResult = options.aircraftListFilter.hookEnabledChanged(this._filterEnabledChanged, this);
            }

            this._showImage();
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;

            if(state.pausedChangedHookResult) {
                options.aircraftListFetcher.unhook(state.pausedChangedHookResult);
                state.pausedChangedHookResult = null;
            }

            if(state.filterEnabledChangedHookResult) {
                options.aircraftListFilter.unhook(state.filterEnabledChangedHookResult);
                state.filterEnabledChangedHookResult = null;
            }

            if(state.imageElement) {
                state.imageElement.off();
                state.imageElement.remove();
                state.imageElement = null;
            }

            this.element.removeClass(VRS.globalOptions.mapNextPageButtonClass);
        }

        /**
         * Updates the image button to reflect changes to the state.
         */
        private _showImage()
        {
            var state = this._getState();
            var options = this.options;

            var image: string = VRS.globalOptions.mapNextPageButtonImage;
            var size: ISize = VRS.globalOptions.mapNextPageButtonSize;

            if(options.aircraftListFetcher && options.aircraftListFetcher.getPaused()) {
                image = VRS.globalOptions.mapNextPageButtonPausedImage;
                size = VRS.globalOptions.mapNextPageButtonPausedSize;
            } else if(options.aircraftListFilter && options.aircraftListFilter.getEnabled()) {
                image = VRS.globalOptions.mapNextPageButtonFilteredImage;
                size = VRS.globalOptions.mapNextPageButtonFilteredSize;
            }

            state.imageElement.prop('width', size.width);
            state.imageElement.prop('height', size.height);
            state.imageElement.prop('src', image);
        }

        /**
         * Called when the button is clicked.
         */
        private _buttonClicked(event: Event)
        {
            VRS.pageManager.show(this.options.nextPageName);
            event.stopPropagation();
            return false;
        }

        /**
         * Called when the paused state changes.
         */
        private _pausedChanged()
        {
            this._showImage();
        }

        /**
         * Called when the enabled state of the filter changes.
         */
        private _filterEnabledChanged()
        {
            this._showImage();
        }
    }

    $.widget('vrs.vrsMapNextPageButton', new MapNextPageButton());
}

declare interface JQuery
{
    vrsMapNextPageButton();
    vrsMapNextPageButton(options: VRS.MapNextPageButton_Options);
    vrsMapNextPageButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
