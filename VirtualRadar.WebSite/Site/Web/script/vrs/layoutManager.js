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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview Code that handles the laying out of the panels within splitters.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region VRS.Layout
    /**
     * Describes a display layout.
     * @param {object}              settings
     * @param {string}              settings.name           The name of the layout. Must be unique.
     * @param {string}              settings.labelKey       The index into VRS.$$ of the text used to describe the layout.
     * @param {VRS_LAYOUT_ARRAY}    settings.layout         The description of the layout.
     * @param {function()}         [settings.onFocus]       Called after the user has selected the layout, but before it is shown to the user.
     * @param {function()}         [settings.onBlur]        Called after the user has selected another layout, after this layout has been torn down but before the new layout is shown.
     * @constructor
     */
    VRS.Layout = function(settings)
    {
        if(!settings) throw 'You must supply a settings object';
        if(!settings.name) throw 'The layout must be named';
        if(!settings.labelKey) throw 'The layout must have a label';
        if(!settings.layout) throw 'The layout must declare a layout';
        if(!(settings.layout instanceof Array) || settings.layout.length != 3) throw 'The layout must be an array of 3 elements';

        this.name = settings.name;
        this.labelKey = settings.labelKey;
        this.layout = settings.layout;
        this.onFocus = settings.onFocus || function() { };
        this.onBlur = settings.onBlur || function() { };
    };
    //endregion

    //region LayoutManager
    /**
     * Manages the creation and destruction of splitters, and holds onto the list of registered splitter layouts.
     * @param {string} [name] The name to use when saving state for the layout manager.
     * @constructor
     */
    VRS.LayoutManager = function(name)
    {
        //region -- Fields
        /**
         * An array of layouts registered with registerLayout().
         * @type {Array.<VRS.Layout>}
         * @private
         */
        var _Layouts = [];

        /**
         * The currently applied layout, if any.
         * @type {{
         * layout:                      VRS.Layout,
         * splitterGroupPersistence:    VRS.SplitterGroupPersistence,
         * topSplitter:                 jQuery,
         * topSplitterIsSplitter:       bool=
         * }}
         */
        var _CurrentLayout;
        //endregion

        //region -- Properties
        var _Name = /** @type {string} */ name || 'default';
        this.getName = function() { return _Name; };

        /** @type {jQuery} @private */
        var _SplitterParent = $('body');
        this.getSplitterParent = function() { return _SplitterParent; };
        this.setSplitterParent = function(/** jQuery */value) {
            if(_CurrentLayout) throw 'You cannot change the splitter parent once a layout has been applied';
            _SplitterParent = value;
            _SplitterParent.css({
                width:  '100%',
                height: '100%',
                position: 'fixed',
                top: '0,',
                left: '0'
            })
        };

        /**
         * Gets the name of the currently selected layout.
         * @returns {string}
         */
        this.getCurrentLayout = function() { return _CurrentLayout ? _CurrentLayout.layout.name : null; };
        //endregion

        //region -- applyLayout, doApplyLayout, undoLayout
        /**
         * Destroys the existing layout and applies a new one.
         * @param {VRS.Layout|string} layoutOrName
         * @param {jQuery} [splitterParent]
         */
        this.applyLayout = function(layoutOrName, splitterParent)
        {
            var layout = /** @type {VRS.Layout} */ layoutOrName;
            if(!(layout instanceof Array)) layout = findLayout(layoutOrName);
            if(layout === null) throw 'Cannot find a layout with a name of ' + layoutOrName;

            undoLayout();
            layout.onFocus();

            var splitterGroupPersistence = new VRS.SplitterGroupPersistence(_Name + '-' + layout.name);

            _CurrentLayout = {
                layout: layout,
                splitterGroupPersistence: splitterGroupPersistence,
                topSplitter: doApplyLayout(layout.layout, splitterParent, splitterGroupPersistence, true)
            };
            _CurrentLayout.topSplitterIsSplitter = !!VRS.jQueryUIHelper.getSplitterPlugin(_CurrentLayout.topSplitter);

            splitterGroupPersistence.setAutoSaveEnabled(true);
        };

        /**
         * Called recursively create a single splitter.
         * @param {VRS_LAYOUT_ARRAY}                layout
         * @param {jQuery}                          splitterParent
         * @param {VRS.SplitterGroupPersistence}    splitterGroupPersistence
         * @param {bool}                            isTopLevelSplitter
         * @returns {jQuery}
         */
        function doApplyLayout(layout, splitterParent, splitterGroupPersistence, isTopLevelSplitter)
        {
            if(!(layout instanceof Array) || layout.length != 3) throw 'The layout must be an array of 3 elements';
            if(!splitterParent) splitterParent = _SplitterParent;

            var leftTop = layout[0];
            var splitterSettings = layout[1];
            var rightBottom = layout[2];

            if(leftTop instanceof Array)     leftTop =     doApplyLayout(leftTop, splitterParent, splitterGroupPersistence, false);
            if(rightBottom instanceof Array) rightBottom = doApplyLayout(rightBottom, splitterParent, splitterGroupPersistence, false);

            var result = null;
            if(!leftTop) result = rightBottom;
            else if(!rightBottom) result = leftTop;
            else {
                splitterSettings.leftTopParent = leftTop.parent();
                splitterSettings.rightBottomParent = rightBottom.parent();
                splitterSettings.splitterGroupPersistence = splitterGroupPersistence;
                splitterSettings.isTopLevelSplitter = isTopLevelSplitter;

                result = $('<div/>')
                    .appendTo(splitterParent);
                leftTop.appendTo(result);
                rightBottom.appendTo(result);
                result.vrsSplitter(splitterSettings);
            }

            return result;
        }

        /**
         * Destroys the current layout.
         */
        function undoLayout()
        {
            if(_CurrentLayout) {
                if(_CurrentLayout.splitterGroupPersistence) {
                    _CurrentLayout.splitterGroupPersistence.dispose();
                }

                if(_CurrentLayout.topSplitter && _CurrentLayout.topSplitterIsSplitter) {
                    _CurrentLayout.topSplitter.vrsSplitter('destroy');
                }

                _CurrentLayout.layout.onBlur();
            }
            _CurrentLayout = null;
        }
        //endregion

        //region -- registerLayout, removeLayoutByName, getLayouts, findLayout, findLayoutIndex
        /**
         * Registers a layout with the layout manager.
         * @param {VRS.Layout} layout
         */
        this.registerLayout = function(layout)
        {
            if(!layout) throw 'You must supply a layout object';

            var existingLayout = findLayout(layout.name);
            if(existingLayout) throw 'There is already a layout called ' + layout.name;

            _Layouts.push(layout);
        };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Removes a registered layout.
         * @param {string} name The name of the layout to remove.
         */
        this.removeLayoutByName = function(name)
        {
            var layoutIndex = findLayoutIndex(name);
            if(layoutIndex !== -1) _Layouts.splice(layoutIndex, 1);
        };

        /**
         * Returns a collection of all registered layouts.
         * @returns {VRS_LAYOUT_LABEL[]}
         */
        this.getLayouts = function()
        {
            var result = [];

            $.each(_Layouts, function() {
                result.push({
                    name: this.name,
                    labelKey: this.labelKey
                });
            });

            return result;
        };

        /**
         * Returns the layout with the given name or null if no such layout exists.
         * @param {string} name The name of the layout to find.
         * @returns {VRS.Layout}
         */
        function findLayout(name)
        {
            var idx = findLayoutIndex(name);
            return idx === -1 ? null : _Layouts[idx];
        }

        /**
         * Returns the index of the layout with the given name or -1 if no such layout exists.
         * @param {string} name The name of the layout to find.
         * @returns {number}
         */
        function findLayoutIndex(name)
        {
            var result = -1;
            $.each(_Layouts, function(idx) {
                if(this.name === name) result = idx;
                return result === -1;
            });

            return result;
        }
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the manager.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the previously saved state or the current state if no state was previously saved.
         * @returns {VRS_STATE_LAYOUTMANAGER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);

            if(result.currentLayout) {
                var existing = findLayout(result.currentLayout);
                if(!existing) result.currentLayout = null;
            }

            return result;
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_LAYOUTMANAGER} settings
         */
        this.applyState = function(settings)
        {
            var layout = settings.currentLayout ? findLayout(settings.currentLayout) : null;
            if(layout) this.applyLayout(layout.name);
        };

        /**
         * Loads and applies the previously saved state.
         */
        this.loadAndApplyState = function()
        {
            this.applyState(this.loadState());
        };

        /**
         * Returns the key that the state will be saved against.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsLayoutManager-' + _Name;
        }

        /**
         * Returns the current state of the object.
         * @returns {VRS_STATE_LAYOUTMANAGER}
         */
        function createSettings()
        {
            return {
                currentLayout: _CurrentLayout ? _CurrentLayout.layout.name : null
            };
        }
        //endregion
    };
    //endregion

    //region Pre-builts
    VRS.layoutManager = new VRS.LayoutManager();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));