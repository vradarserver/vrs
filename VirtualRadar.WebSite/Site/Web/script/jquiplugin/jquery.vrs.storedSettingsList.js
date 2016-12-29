var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var StoredSettingsList_State = (function () {
        function StoredSettingsList_State() {
            this.keysContainer = null;
            this.keyContentContainer = null;
            this.currentKey = null;
            this.importExportElement = null;
            this.importControlsContainer = null;
        }
        return StoredSettingsList_State;
    }());
    var StoredSettingsList = (function (_super) {
        __extends(StoredSettingsList, _super);
        function StoredSettingsList() {
            _super.apply(this, arguments);
        }
        StoredSettingsList.prototype._getState = function () {
            var result = this.element.data('storedSettingsState');
            if (result === undefined) {
                result = new StoredSettingsList_State();
                this.element.data('storedSettingsState', result);
            }
            return result;
        };
        StoredSettingsList.prototype._create = function () {
            var _this = this;
            var state = this._getState();
            this.element.addClass('vrsStoredSettings');
            var buttonBlock = $('<div/>')
                .appendTo(this.element);
            $('<button />')
                .text(VRS.$$.RemoveAll)
                .click(function () { return _this._removeAllKeys(); })
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.Refresh)
                .click(function () { return _this._refreshDisplay(); })
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ExportSettings)
                .click(function () { return _this._exportSettings(); })
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ImportSettings)
                .click(function () { return _this._showImportControls(); })
                .appendTo(buttonBlock);
            var importExport = $('<div />')
                .attr('class', 'importExport')
                .appendTo(this.element);
            state.importExportElement = $('<textarea />')
                .hide()
                .appendTo(importExport);
            state.importControlsContainer =
                $('<div />')
                    .hide()
                    .attr('class', 'import')
                    .appendTo(importExport);
            var checkboxesContainer = $('<ol />')
                .appendTo(state.importControlsContainer);
            var importOverwrite = this._addCheckBox(checkboxesContainer, VRS.$$.OverwriteExistingSettings, true);
            var importReset = this._addCheckBox(checkboxesContainer, VRS.$$.EraseBeforeImport, true);
            var importIgnoreRequestFeedId = this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportRequestFeedId, true);
            var importIgnoreLanguage = this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportLanguageSettings, false);
            var importIgnoreSplitters = this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportSplitters, false);
            var importIgnoreCurrentLocation = this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportCurrentLocation, false);
            var importIgnoreAutoSelect = this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportAutoSelect, false);
            var importButton = $('<button />')
                .text(VRS.$$.Import)
                .click(function () {
                return _this._importSettings({
                    overwrite: importOverwrite.prop('checked'),
                    resetBeforeImport: importReset.prop('checked'),
                    ignoreLanguage: importIgnoreLanguage.prop('checked'),
                    ignoreSplitters: importIgnoreSplitters.prop('checked'),
                    ignoreCurrentLocation: importIgnoreCurrentLocation.prop('checked'),
                    ignoreAutoSelect: importIgnoreAutoSelect.prop('checked'),
                    ignoreRequestFeedId: importIgnoreRequestFeedId.prop('checked')
                });
            })
                .appendTo(state.importControlsContainer);
            state.keysContainer =
                $('<div/>')
                    .addClass('keys')
                    .appendTo(this.element);
            state.keyContentContainer =
                $('<div/>')
                    .addClass('content')
                    .appendTo(this.element);
            this._buildKeysTable(state);
        };
        StoredSettingsList.prototype._addCheckBox = function (parentElement, labelText, initialCheckedState) {
            var listItem = $('<li />')
                .appendTo(parentElement);
            var result = $('<input />')
                .uniqueId()
                .attr('type', 'checkbox')
                .prop('checked', initialCheckedState)
                .appendTo(listItem);
            $('<label />')
                .attr('for', result.attr('id'))
                .text(labelText)
                .appendTo(listItem);
            return result;
        };
        StoredSettingsList.prototype._buildKeysTable = function (state) {
            var _this = this;
            state.keysContainer.empty();
            var statistics = $('<table/>')
                .addClass('statistics')
                .appendTo(state.keysContainer)
                .append($('<tr/>')
                .append($('<td/>').text(VRS.$$.StorageEngine + ':'))
                .append($('<td/>').text(VRS.configStorage.getStorageEngine())))
                .append($('<tr/>')
                .append($('<td/>').text(VRS.$$.StorageSize + ':'))
                .append($('<td/>').text(VRS.configStorage.getStorageSize())));
            var list = $('<ul/>')
                .appendTo(state.keysContainer);
            var hasContent = false;
            var keys = VRS.configStorage.getAllVirtualRadarKeys().sort();
            $.each(keys, function (idx, key) {
                hasContent = true;
                var keyName = String(key);
                var listItem = $('<li/>')
                    .text(keyName)
                    .click(function () { return _this._keyClicked(keyName); })
                    .appendTo(list);
                if (keyName === state.currentKey) {
                    listItem.addClass('current');
                }
            });
            if (!hasContent) {
                list.append($('<li/>')
                    .text(VRS.$$.NoSettingsFound)
                    .addClass('empty'));
            }
        };
        StoredSettingsList.prototype._showKeyContent = function (keyName, content) {
            var _this = this;
            var state = this._getState();
            var container = state.keyContentContainer;
            var self = this;
            state.currentKey = keyName;
            container.empty();
            if (keyName) {
                $('<p/>')
                    .addClass('keyTitle')
                    .text(keyName)
                    .appendTo(container);
                var contentDump = $('<code/>').appendTo(container);
                if (content === null || content === undefined) {
                    contentDump
                        .addClass('empty')
                        .text(content === null ? '<null>' : '<undefined>');
                }
                else {
                    var json = $.toJSON(content);
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
                    .click(function () { return _this._removeKey(keyName); })
                    .appendTo(buttonBlock);
            }
            this._buildKeysTable(state);
        };
        StoredSettingsList.prototype._refreshDisplay = function () {
            var state = this._getState();
            var content = null;
            if (state.currentKey) {
                var exists = false;
                $.each(VRS.configStorage.getAllVirtualRadarKeys(), function () {
                    exists = String(this) === state.currentKey;
                    return !exists;
                });
                if (!exists) {
                    state.currentKey = null;
                }
                else {
                    content = VRS.configStorage.getContentWithoutPrefix(state.currentKey);
                }
            }
            this._showKeyContent(state.currentKey, content);
        };
        StoredSettingsList.prototype._removeKey = function (keyName) {
            var state = this._getState();
            VRS.configStorage.removeContentWithoutPrefix(keyName);
            state.currentKey = null;
            this._buildKeysTable(state);
            state.keyContentContainer.empty();
        };
        StoredSettingsList.prototype._removeAllKeys = function () {
            var state = this._getState();
            state.currentKey = null;
            VRS.configStorage.removeAllContent();
            state.keyContentContainer.empty();
            this._buildKeysTable(state);
        };
        StoredSettingsList.prototype._exportSettings = function () {
            var state = this._getState();
            state.importControlsContainer.hide();
            var element = state.importExportElement;
            if (element.is(':visible')) {
                element.hide();
            }
            else {
                element.val('');
                element.show();
                var settings = VRS.configStorage.exportSettings();
                element.val(settings);
            }
        };
        StoredSettingsList.prototype._showImportControls = function () {
            var state = this._getState();
            var element = state.importExportElement;
            if (!element.is(':visible')) {
                element.show();
                element.val('');
                state.importControlsContainer.show();
            }
            else {
                element.hide();
                state.importControlsContainer.hide();
            }
        };
        StoredSettingsList.prototype._importSettings = function (options) {
            var state = this._getState();
            var text = state.importExportElement.val();
            if (text) {
                state.currentKey = null;
                try {
                    VRS.configStorage.importSettings(text, options);
                    this._refreshDisplay();
                }
                catch (ex) {
                    VRS.pageHelper.showMessageBox(VRS.$$.ImportFailedTitle, VRS.stringUtility.format(VRS.$$.ImportFailedBody, ex));
                }
            }
        };
        StoredSettingsList.prototype._keyClicked = function (keyName) {
            var content = VRS.configStorage.getContentWithoutPrefix(keyName);
            this._showKeyContent(keyName, content);
        };
        return StoredSettingsList;
    }(JQueryUICustomWidget));
    VRS.StoredSettingsList = StoredSettingsList;
    $.widget('vrs.vrsStoredSettingsList', new StoredSettingsList());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.storedSettingsList.js.map