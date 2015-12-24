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
 * @fileoverview Code that handles the laying out of the panels within splitters.
 */

namespace VRS
{
    /**
     * Describes a layout.
     */
    export interface Layout_Options
    {
        name:           string;
        vertical:       boolean;
        savePane:       number;
        collapsePane?:  number;
        maxPane?:       number;
        max?:           number|string;
        startSizePane?: number;
        startSize?:     number|string;
        fixedPane?:     number;
    }

    /**
     * Describes an array that contains a layout. The array must be three elements long. The first element is the top or left
     * panel in the splitter (usually a JQuery), the middle element is an instance of Layout_Options and the third element is
     * the bottom or right panel. Each panel can itself be a Layout Array.
     */
    export type Layout_Array = [JQuery | any[], Layout_Options, JQuery | any[]];  // Typescript doesn't allow circular type references, even in tuples

    /**
     * The settings to pass when creating a new Layout.
     */
    export interface Layout_Settings
    {
        /**
         * The name of the layout. Must be unique.
         */
        name: string;

        /**
         * The index into VRS.$$ of the text used to describe the layout.
         */
        labelKey: string;

        /**
         * The description of the layout.
         */
        layout: Layout_Array;

        /**
         * Called after the user has selected the layout, but before it is shown to the user.
         */
        onFocus?: () => void;

        /**
         * Called after the user has selected another layout, after this layout has been torn down but before the new layout is shown.
         */
        onBlur?: () => void;
    }

    /**
     * Describes a display layout.
     */
    export class Layout
    {
        private _Settings: Layout_Settings;

        constructor(settings: Layout_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.name) throw 'The layout must be named';
            if(!settings.labelKey) throw 'The layout must have a label';
            if(!settings.layout) throw 'The layout must declare a layout';
            if(!(settings.layout instanceof Array) || settings.layout.length != 3) throw 'The layout must be an array of 3 elements';

            settings.onFocus = settings.onFocus || $.noop;
            settings.onBlur = settings.onBlur || $.noop;

            this._Settings = settings;
        }

        get name()
        {
            return this._Settings.name;
        }

        get labelKey()
        {
            return this._Settings.labelKey;
        }

        get layout()
        {
            return this._Settings.layout;
        }

        get onFocus()
        {
            return this._Settings.onFocus;
        }

        get onBlur()
        {
            return this._Settings.onBlur;
        }
    }

    /**
     * Describes the currently selected layout.
     */
    interface Layout_Detail
    {
        layout:                      Layout;
        splitterGroupPersistence:    SplitterGroupPersistence;
        topSplitter:                 JQuery;
        topSplitterIsSplitter?:      boolean;
    }

    /**
     * Records the name of a layout and its label.
     */
    export interface LayoutNameAndLabel
    {
        name:            string;
        labelKey:        string;
    }

    /**
     * The settings saved by the LayoutManager.
     */
    export interface LayoutManager_SaveState
    {
        currentLayout: string;
    }

    /**
     * Manages the creation and destruction of splitters, and holds onto the list of registered splitter layouts.
     */
    export class LayoutManager implements ISelfPersist<LayoutManager_SaveState>
    {
        private _Layouts: Layout[] = [];        // An array of layouts registered with registerLayout().
        private _CurrentLayout: Layout_Detail;
        private _Name: string;
        private _SplitterParent: JQuery;

        constructor(name?: string)
        {
            this._Name = name || 'default';
            this._SplitterParent = $('body');
        }

        getName() : string
        {
            return this._Name;
        }

        getSplitterParent() : JQuery
        {
            return this._SplitterParent;
        }
        setSplitterParent(value: JQuery)
        {
            if(this._CurrentLayout) throw 'You cannot change the splitter parent once a layout has been applied';
            this._SplitterParent = value;
            this._SplitterParent.css({
                width:  '100%',
                height: '100%',
                position: 'fixed',
                top: '0,',
                left: '0'
            })
        }

        /**
         * Gets the name of the currently selected layout.
         */
        getCurrentLayout() : string
        {
            return this._CurrentLayout ? this._CurrentLayout.layout.name : null;
        }

        /**
         * Destroys the existing layout and applies a new one.
         */
        applyLayout(layoutOrName: string | Layout, splitterParent?: JQuery)
        {
            var layout: Layout = <Layout>layoutOrName;
            if(!(layout instanceof Array)) {
                layout = this.findLayout(<string>layoutOrName);
            }
            if(layout === null) {
                throw 'Cannot find a layout with a name of ' + <string>layoutOrName;
            }

            this.undoLayout();
            layout.onFocus();

            var splitterGroupPersistence = new VRS.SplitterGroupPersistence(this._Name + '-' + layout.name);

            this._CurrentLayout = {
                layout:                     layout,
                splitterGroupPersistence:   splitterGroupPersistence,
                topSplitter:                this.doApplyLayout(layout.layout, splitterParent, splitterGroupPersistence, true)
            };
            this._CurrentLayout.topSplitterIsSplitter = !!VRS.jQueryUIHelper.getSplitterPlugin(this._CurrentLayout.topSplitter);

            splitterGroupPersistence.setAutoSaveEnabled(true);
        }

        /**
         * Called recursively create a single splitter.
         */
        private doApplyLayout(layout: Layout_Array, splitterParent, splitterGroupPersistence, isTopLevelSplitter) : JQuery
        {
            if(!(layout instanceof Array) || layout.length != 3) throw 'The layout must be an array of 3 elements';
            if(!splitterParent) {
                splitterParent = this._SplitterParent;
            }

            var leftTop: JQuery = <JQuery>layout[0];
            var splitterSettings: Splitter_Options = <Layout_Options>layout[1];
            var rightBottom: JQuery = <JQuery>layout[2];

            if(leftTop instanceof Array)     leftTop =     this.doApplyLayout(<any>leftTop, splitterParent, splitterGroupPersistence, false);
            if(rightBottom instanceof Array) rightBottom = this.doApplyLayout(<any>rightBottom, splitterParent, splitterGroupPersistence, false);

            var result = null;
            if(!leftTop) {
                result = rightBottom;
            } else if(!rightBottom) {
                result = leftTop;
            } else {
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
        private undoLayout()
        {
            if(this._CurrentLayout) {
                if(this._CurrentLayout.splitterGroupPersistence) {
                    this._CurrentLayout.splitterGroupPersistence.dispose();
                }

                if(this._CurrentLayout.topSplitter && this._CurrentLayout.topSplitterIsSplitter) {
                    this._CurrentLayout.topSplitter.vrsSplitter('destroy');
                }

                this._CurrentLayout.layout.onBlur();
            }
            this._CurrentLayout = null;
        }

        /**
         * Registers a layout with the layout manager.
         */
        registerLayout(layout: Layout)
        {
            if(!layout) throw 'You must supply a layout object';

            var existingLayout = this.findLayout(layout.name);
            if(existingLayout) throw 'There is already a layout called ' + layout.name;

            this._Layouts.push(layout);
        }

        /**
         * Removes a registered layout.
         */
        removeLayoutByName(name: string)
        {
            var layoutIndex = this.findLayoutIndex(name);
            if(layoutIndex !== -1) {
                this._Layouts.splice(layoutIndex, 1);
            }
        }

        /**
         * Returns a collection of all registered layouts.
         */
        getLayouts() : LayoutNameAndLabel[]
        {
            var result: LayoutNameAndLabel[] = [];

            $.each(this._Layouts, function() {
                result.push({
                    name:       this.name,
                    labelKey:   this.labelKey
                });
            });

            return result;
        }

        /**
         * Returns the layout with the given name or null if no such layout exists.
         */
        private findLayout(name: string) : Layout
        {
            var idx = this.findLayoutIndex(name);
            return idx === -1 ? null : this._Layouts[idx];
        }

        /**
         * Returns the index of the layout with the given name or -1 if no such layout exists.
         */
        private findLayoutIndex(name: string) : number
        {
            var result = -1;
            $.each(this._Layouts, function(idx) {
                if(this.name === name) {
                    result = idx;
                }
                return result === -1;
            });

            return result;
        }

        /**
         * Saves the current state of the manager.
         */
        saveState = function()
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the previously saved state or the current state if no state was previously saved.
         */
        loadState() : LayoutManager_SaveState
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);

            if(result.currentLayout) {
                var existing = this.findLayout(result.currentLayout);
                if(!existing) {
                    result.currentLayout = null;
                }
            }

            return result;
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState(settings: LayoutManager_SaveState)
        {
            var layout = settings.currentLayout ? this.findLayout(settings.currentLayout) : null;
            if(layout) this.applyLayout(layout.name);
        }

        /**
         * Loads and applies the previously saved state.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key that the state will be saved against.
         */
        private persistenceKey() : string
        {
            return 'vrsLayoutManager-' + this._Name;
        }

        /**
         * Returns the current state of the object.
         */
        private createSettings() : LayoutManager_SaveState
        {
            return {
                currentLayout: this._CurrentLayout ? this._CurrentLayout.layout.name : null
            };
        }
    }

    /*
     * Pre-builts
     */
    export var layoutManager = new VRS.LayoutManager();
}
