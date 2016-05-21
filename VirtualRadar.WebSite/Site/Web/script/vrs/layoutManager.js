var VRS;
(function (VRS) {
    var Layout = (function () {
        function Layout(settings) {
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.name)
                throw 'The layout must be named';
            if (!settings.labelKey)
                throw 'The layout must have a label';
            if (!settings.layout)
                throw 'The layout must declare a layout';
            if (!(settings.layout instanceof Array) || settings.layout.length != 3)
                throw 'The layout must be an array of 3 elements';
            this.name = settings.name;
            this.labelKey = settings.labelKey;
            this.layout = settings.layout;
            this.onFocus = settings.onFocus || function () { };
            this.onBlur = settings.onBlur || function () { };
        }
        return Layout;
    }());
    VRS.Layout = Layout;
    var LayoutManager = (function () {
        function LayoutManager(name) {
            this._Layouts = [];
            this.saveState = function () {
                VRS.configStorage.save(this.persistenceKey(), this.createSettings());
            };
            this._Name = name || 'default';
            this._SplitterParent = $('body');
        }
        LayoutManager.prototype.getName = function () {
            return this._Name;
        };
        LayoutManager.prototype.getSplitterParent = function () {
            return this._SplitterParent;
        };
        LayoutManager.prototype.setSplitterParent = function (value) {
            if (this._CurrentLayout)
                throw 'You cannot change the splitter parent once a layout has been applied';
            this._SplitterParent = value;
            this._SplitterParent.css({
                width: '100%',
                height: '100%',
                position: 'fixed',
                top: '0,',
                left: '0'
            });
        };
        LayoutManager.prototype.getCurrentLayout = function () {
            return this._CurrentLayout ? this._CurrentLayout.layout.name : null;
        };
        LayoutManager.prototype.applyLayout = function (layoutOrName, splitterParent) {
            var layout = layoutOrName;
            if (!(layout instanceof Array)) {
                layout = this.findLayout(layoutOrName);
            }
            if (layout === null) {
                throw 'Cannot find a layout with a name of ' + layoutOrName;
            }
            this.undoLayout();
            layout.onFocus();
            var splitterGroupPersistence = new VRS.SplitterGroupPersistence(this._Name + '-' + layout.name);
            this._CurrentLayout = {
                layout: layout,
                splitterGroupPersistence: splitterGroupPersistence,
                topSplitter: this.doApplyLayout(layout.layout, splitterParent, splitterGroupPersistence, true)
            };
            this._CurrentLayout.topSplitterIsSplitter = !!VRS.jQueryUIHelper.getSplitterPlugin(this._CurrentLayout.topSplitter);
            splitterGroupPersistence.setAutoSaveEnabled(true);
        };
        LayoutManager.prototype.doApplyLayout = function (layout, splitterParent, splitterGroupPersistence, isTopLevelSplitter) {
            if (!(layout instanceof Array) || layout.length != 3)
                throw 'The layout must be an array of 3 elements';
            if (!splitterParent) {
                splitterParent = this._SplitterParent;
            }
            var leftTop = layout[0];
            var splitterSettings = layout[1];
            var rightBottom = layout[2];
            if (leftTop instanceof Array)
                leftTop = this.doApplyLayout(leftTop, splitterParent, splitterGroupPersistence, false);
            if (rightBottom instanceof Array)
                rightBottom = this.doApplyLayout(rightBottom, splitterParent, splitterGroupPersistence, false);
            var result = null;
            if (!leftTop) {
                result = rightBottom;
            }
            else if (!rightBottom) {
                result = leftTop;
            }
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
        };
        LayoutManager.prototype.undoLayout = function () {
            if (this._CurrentLayout) {
                if (this._CurrentLayout.splitterGroupPersistence) {
                    this._CurrentLayout.splitterGroupPersistence.dispose();
                }
                if (this._CurrentLayout.topSplitter && this._CurrentLayout.topSplitterIsSplitter) {
                    this._CurrentLayout.topSplitter.vrsSplitter('destroy');
                }
                this._CurrentLayout.layout.onBlur();
            }
            this._CurrentLayout = null;
        };
        LayoutManager.prototype.registerLayout = function (layout) {
            if (!layout)
                throw 'You must supply a layout object';
            var existingLayout = this.findLayout(layout.name);
            if (existingLayout)
                throw 'There is already a layout called ' + layout.name;
            this._Layouts.push(layout);
        };
        LayoutManager.prototype.removeLayoutByName = function (name) {
            var layoutIndex = this.findLayoutIndex(name);
            if (layoutIndex !== -1) {
                this._Layouts.splice(layoutIndex, 1);
            }
        };
        LayoutManager.prototype.getLayouts = function () {
            var result = [];
            $.each(this._Layouts, function () {
                result.push({
                    name: this.name,
                    labelKey: this.labelKey
                });
            });
            return result;
        };
        LayoutManager.prototype.findLayout = function (name) {
            var idx = this.findLayoutIndex(name);
            return idx === -1 ? null : this._Layouts[idx];
        };
        LayoutManager.prototype.findLayoutIndex = function (name) {
            var result = -1;
            $.each(this._Layouts, function (idx) {
                if (this.name === name) {
                    result = idx;
                }
                return result === -1;
            });
            return result;
        };
        LayoutManager.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);
            if (result.currentLayout) {
                var existing = this.findLayout(result.currentLayout);
                if (!existing) {
                    result.currentLayout = null;
                }
            }
            return result;
        };
        LayoutManager.prototype.applyState = function (settings) {
            var layout = settings.currentLayout ? this.findLayout(settings.currentLayout) : null;
            if (layout)
                this.applyLayout(layout.name);
        };
        LayoutManager.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        LayoutManager.prototype.persistenceKey = function () {
            return 'vrsLayoutManager-' + this._Name;
        };
        LayoutManager.prototype.createSettings = function () {
            return {
                currentLayout: this._CurrentLayout ? this._CurrentLayout.layout.name : null
            };
        };
        return LayoutManager;
    }());
    VRS.LayoutManager = LayoutManager;
    VRS.layoutManager = new VRS.LayoutManager();
})(VRS || (VRS = {}));
//# sourceMappingURL=layoutManager.js.map