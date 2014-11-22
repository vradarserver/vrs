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
 * @fileoverview A jQuery UI widget that displays all of the configuration settings held in persistent storage and allows the user to manipulate them.
 */

(function(VRS, $, /** object= */undefined)
{
    //region StoredSettingsPluginState
    /**
     * The state object for the stored settings plugin.
     * @constructor
     */
    VRS.StoredSettingsPluginState = function()
    {
        /**
         * The container that lists all of the VRS keys.
         * @type {jQuery=}
         */
        this.keysContainer = null;

        /**
         * The container that holds the content of a single key.
         * @type {jQuery=}
         */
        this.keyContentContainer = null;

        /**
         * The currently selected key.
         * @type {string}
         */
        this.currentKey = null;

        /**
         * The textarea element that shows the import/export settings.
         * @type {jQuery=}
         */
        this.importExportElement = null;

        /**
         * The button that can be used to import settings.
         * @type {jQuery=}
         */
        this.importButtonElement = null;
    };
    //endregion

    //region vrsStoredSettingsList
    /**
     * A jQuery UI widget that can show the user all of the settings stored at the browser and let them manipulate them.
     * @namespace VRS.vrsStoredSettingsList
     */
    $.widget('vrs.vrsStoredSettingsList', {
        //region -- options
        options: {
            __nop: null
        },
        //endregion

        //region -- _create
        /**
         * Returns the state object for the widget, creating it if it doesn't already exist.
         * @returns {VRS.StoredSettingsPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('storedSettingsState');
            if(result === undefined) {
                result = new VRS.StoredSettingsPluginState();
                this.element.data('storedSettingsState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var state = this._getState();
            var self = this;

            this.element.addClass('vrsStoredSettings');

            var buttonBlock =
                $('<div/>')
                    .appendTo(this.element);
            $('<button />')
                .text(VRS.$$.RemoveAll)
                .click($.proxy(function() {
                    self._removeAllKeys();
                }, this))
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.Refresh)
                .click($.proxy(function() {
                    self._refreshDisplay();
                }, this))
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ExportSettings)
                .click($.proxy(function() {
                    self._exportSettings();
                }, this))
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ImportSettings)
                .click($.proxy(function() {
                    self._showImportControls();
                }, this))
                .appendTo(buttonBlock);

            var importExport =
                $('<div />')
                    .attr('class', 'importExport')
                    .appendTo(this.element);
            state.importExportElement = $('<textarea />')
                .hide()
                .appendTo(importExport);
            state.importButtonElement = $('<button />')
                .hide()
                .text(VRS.$$.Import)
                .click($.proxy(function() {
                    self._importSettings();
                }, this))
                .appendTo(importExport);

            state.keysContainer =
                $('<div/>')
                    .addClass('keys')
                    .appendTo(this.element);
            state.keyContentContainer =
                $('<div/>')
                    .addClass('content')
                    .appendTo(this.element);

            this._buildKeysTable(state);
        },

        /**
         * Builds a table to display the keys that are in use.
         * @param {VRS.StoredSettingsPluginState} state
         * @private
         */
        _buildKeysTable: function(state)
        {
            state.keysContainer.empty();

            var statistics =
                $('<table/>')
                    .addClass('statistics')
                    .appendTo(state.keysContainer)
                    .append(
                        $('<tr/>')
                            .append($('<td/>').text(VRS.$$.StorageEngine + ':'))
                            .append($('<td/>').text(VRS.configStorage.getStorageEngine()))
                    )
                    .append(
                        $('<tr/>')
                            .append($('<td/>').text(VRS.$$.StorageSize + ':'))
                            .append($('<td/>').text(VRS.configStorage.getStorageSize()))
                    );
            var list = $('<ul/>')
                .appendTo(state.keysContainer);

            var self = this;
            var hasContent = false;
            var keys = VRS.configStorage.getAllVirtualRadarKeys().sort();
            $.each(keys, function() {
                hasContent = true;
                var keyName = String(this);
                var listItem =
                        $('<li/>')
                            .text(keyName)
                            .click($.proxy(function() { self._keyClicked(keyName); }, self))
                            .appendTo(list);
                if(keyName === state.currentKey) listItem.addClass('current');
            });
            if(!hasContent) {
                list.append(
                    $('<li/>')
                        .text(VRS.$$.NoSettingsFound)
                        .addClass('empty')
                );
            }
        },
        //endregion

        //region -- _showKeyContent, _removeKey, _removeAllKeys
        /**
         * Displays a single key's content to the user.
         * @param {string}  keyName
         * @param {*}       content
         * @private
         */
        _showKeyContent: function(keyName, content)
        {
            var state = this._getState();
            var container = state.keyContentContainer;
            var self = this;

            state.currentKey = keyName;
            container.empty();

            if(keyName) {
                $('<p/>')
                    .addClass('keyTitle')
                    .text(keyName)
                    .appendTo(container);

                var contentDump = $('<code/>').appendTo(container);
                if(content === null || content === undefined) {
                    contentDump
                        .addClass('empty')
                        .text(content === null ? '<null>' : '<undefined>');
                } else {
                    var json = $.toJSON(content, null, 4);
                    json = json
                        .replace(/&/g, '&amp;')
                        .replace(/ /g, '&nbsp')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/\t/g, '&nbsp;&nbsp;&nbsp;&nbsp;')
                        .replace(/\n/g, '<br />');
                    contentDump.html(json);
                }

                var buttonBlock = $('<div/>')
                    .addClass('buttonBlock')
                    .appendTo(container);
                $('<button/>')
                    .text(VRS.$$.Remove)
                    .click($.proxy(function() {
                        self._removeKey(keyName);
                    }, this))
                    .appendTo(buttonBlock);
            }

            this._buildKeysTable(state);
        },

        /**
         * Refreshes the display of keys and content.
         * @private
         */
        _refreshDisplay: function()
        {
            var state = this._getState();

            var content = null;
            if(state.currentKey) {
                var exists = false;
                $.each(VRS.configStorage.getAllVirtualRadarKeys(), function() {
                    exists = String(this) === state.currentKey;
                    return !exists;
                });
                if(!exists) state.currentKey = null;
                else content = VRS.configStorage.getContentWithoutPrefix(state.currentKey);
            }
            this._showKeyContent(state.currentKey, content);
        },

        /**
         * Removes the configuration settings associated with the key passed across.
         * @param {string} keyName
         * @private
         */
        _removeKey: function(keyName)
        {
            var state = this._getState();
            VRS.configStorage.removeContentWithoutPrefix(keyName);
            state.currentKey = null;

            this._buildKeysTable(state);
            state.keyContentContainer.empty();
        },

        /**
         * Removes all configuration settings.
         * @private
         */
        _removeAllKeys: function()
        {
            var state = this._getState();
            state.currentKey = null;
            $.each(VRS.configStorage.getAllVirtualRadarKeys(), function() {
                VRS.configStorage.removeContentWithoutPrefix(this);
            });
            state.keyContentContainer.empty();
            this._buildKeysTable(state);
        },

        /**
         * Creates and displays the serialised settings.
         * @private
         */
        _exportSettings: function()
        {
            var state = this._getState();
            state.importButtonElement.hide();

            var element = state.importExportElement;
            if(element.is(':visible')) {
                element.hide();
            } else {
                element.val('');
                element.show();

                var settings = VRS.configStorage.exportSettings();
                element.val(settings);
            }
        },

        /**
         * Displays the import controls.
         * @private
         */
        _showImportControls: function()
        {
            var state = this._getState();

            var element = state.importExportElement;
            if(!element.is(':visible')) {
                element.show();
                element.val('');
                state.importButtonElement.show();
            } else {
                element.hide();
                state.importButtonElement.hide();
            }
        },

        /**
         * Takes the text in the import textarea and attempts to import it.
         * @private
         */
        _importSettings: function()
        {
            var state = this._getState();

            var text = state.importExportElement.val();
            if(text) {
                state.currentKey = null;
                try {
                    VRS.configStorage.importSettings(text);
                    this._refreshDisplay();
                } catch(ex) {
                    VRS.pageHelper.showMessageBox(VRS.$$.ImportFailedTitle, VRS.stringUtility.format(VRS.$$.ImportFailedBody, ex));
                }
            }
        },
        //endregion

        //region -- Events consumed
        /**
         * Called when the user clicks on an entry for the key in the keys table.
         * @param {string} keyName  The name of the key that was clicked.
         * @private
         */
        _keyClicked: function(keyName)
        {
            var content = VRS.configStorage.getContentWithoutPrefix(keyName);
            this._showKeyContent(keyName, content);
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));

