/**
 * @license Copyright © 2019 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace VRS
{
    /**
     * Describes the settings and current state for a single map layer.
     */
    export class MapLayerSetting
    {
        // Events
        private _Dispatcher = new EventHandler({
            name: 'VRS.MapLayerSetting'
        });
        private _Events = {
            visibilityChanged:      'visibilityChanged',
            opacityOverrideChanged: 'opacityOverrideChanged'
        };

        // Fields
        private _TileServerSettings: ITileServerSettings;
        private _Opacity: number;
        private _Map: IMap;
        private _DisplayOrder: number;      // The ticks when the map was made visible. These can be compared to work out the order in which layers were made visible.
        private _IsSuppressed: boolean;       // True if this has been suppressed from display for some reason.

        //
        // Properties

        get IsVisible(): boolean { return this._Map && this._Map.hasLayer(this.Name); }

        get DisplayOrder(): number { return this.IsVisible ? this._DisplayOrder : -1; }

        get TileServerSettings(): ITileServerSettings { return this._TileServerSettings; }

        get Name(): string { return this._TileServerSettings.Name; }

        get Opacity(): number { return this._Opacity === undefined ? this._TileServerSettings.DefaultOpacity : this._Opacity ; }
        get OpacityDefault(): number { return this._TileServerSettings.DefaultOpacity; }

        get OpacityOverride(): number { return this._Opacity; }
        set OpacityOverride(value: number) {
            var newOpacity = isNaN(value) || value === null || value === undefined || value === this._TileServerSettings.DefaultOpacity
            ? undefined
            : Math.max(0, Math.min(100, value));

            if(newOpacity !== this._Opacity) {
                this._Opacity = newOpacity;
                this.raiseOpacityOverrideChanged();
            }
        }

        get IsSuppressed() : boolean { return this._IsSuppressed; }
        set IsSuppressed(value: boolean) {
            if(this._IsSuppressed != !!value) {
                this._IsSuppressed = !!value;
                if(this.IsSuppressed && this.IsVisible) {
                    this.hide();
                }
            }
        }

        //
        // Event methods

        hookVisibilityChanged(callback: (layer: MapLayerSetting) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.visibilityChanged, callback, forceThis);
        }

        private raiseVisibilityChanged()
        {
            this._Dispatcher.raise(this._Events.visibilityChanged, [ this ]);
        }

        hookOpacityOverrideChanged(callback: (layer: MapLayerSetting) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.opacityOverrideChanged, callback, forceThis);
        }

        private raiseOpacityOverrideChanged()
        {
            this._Dispatcher.raise(this._Events.opacityOverrideChanged, [ this ]);
        }

        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        //
        // Ctor
        constructor(map: IMap, tileServerSettings: ITileServerSettings)
        {
            this._Map = map;
            this._TileServerSettings = tileServerSettings;
        }

        //
        // Visibility methods
        show()
        {
            if(!this.IsVisible && this._Map && !this.IsSuppressed) {
                this._DisplayOrder = new Date().getTime();
                this._Map.addLayer(this.TileServerSettings, this.OpacityOverride);
                this.raiseVisibilityChanged();
            }
        }

        hide()
        {
            if(this.IsVisible && this._Map) {
                this._Map.destroyLayer(this.Name);
                this.raiseVisibilityChanged();
            }
        }

        toggleVisible()
        {
            if(this.IsVisible) {
                this.hide();
            } else {
                this.show();
            }
        }

        //
        // Opacity methods
        getMapOpacity()
        {
            return this.IsVisible ? this._Map.getLayerOpacity(this.Name) : this.Opacity;
        }

        setMapOpacity(value: number)
        {
            this.OpacityOverride = value;
            if(this.IsVisible) {
                this._Map.setLayerOpacity(this.Name, value);
            }
        }
    }

    // Saved state
    export interface MapLayerManager_SaveState
    {
        // The names of the layouts visible at time of save. Order of names indicates order
        // in which they were added to the map.
        visibleLayouts: string[];

        // Associative array of layout names and the opacity override associated with the name.
        opacityOverrides: { [layoutName: string]: number };
    }

    // Records all of the event handles hooked for a map layer.
    class HookHandles
    {
        opacityOverrideChangedEventHandle: IEventHandle;
        visibilityChangedEventHandle: IEventHandle;
    }

    // Singleton object that handles the creation of map MapLayerSetting objects and persistence
    // of their state.
    export class MapLayerManager implements ISelfPersist<MapLayerManager_SaveState>
    {
        // Fields
        private _Map: IMap;
        private _MapLayerSettings: MapLayerSetting[] = [];
        private _HookHandles: { [layoutName: string]: HookHandles } = {};
        private _ConfigurationChangedHook: IEventHandle;
        private _PersistenceKey = 'vrsMapLayerManager';
        private _ApplyingState = false;
        private _CustomMapLayerSettings: ITileServerSettings[] = [];
        private _SuppressedMapLayers: string[] = [];

        /**
         * Called once to register a map to draw layers on.
         * @param map
         */
        registerMap(map: IMap)
        {
            this._Map = map;
            this.buildMapLayerSettings();
            this.loadAndApplyState();

            this._ConfigurationChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.configurationChanged, this);
        }

        /**
         * A method that lets custom content plugins register a layer settings directly instead
         * of adding them to the custom tile server settings JSON file on the server.
         * @param layerTileServerSettings
         */
        registerCustomMapLayerSetting(layerTileServerSettings: ITileServerSettings)
        {
            if(layerTileServerSettings && layerTileServerSettings.IsLayer && layerTileServerSettings.Name) {
                if(!VRS.arrayHelper.findFirst(this._MapLayerSettings, (r) => r.Name === layerTileServerSettings.Name) &&
                   !VRS.arrayHelper.findFirst(this._CustomMapLayerSettings, (r) => r.Name === layerTileServerSettings.Name)
                ) {
                    this._CustomMapLayerSettings.push(layerTileServerSettings);
                    this.buildMapLayerSettings();
                    this.loadAndApplyState();
                }
            }
        }

        /**
         * Suppresses the use / display of a standard layer.
         * @param layerName
         */
        suppressStandardLayer(layerName: string)
        {
            if(layerName) {
                if(VRS.arrayHelper.indexOf(this._SuppressedMapLayers, layerName) === -1) {
                    this._SuppressedMapLayers.push(layerName);

                    var layerTileServerSettings = VRS.arrayHelper.findFirst(this._MapLayerSettings, (r) => r.Name == layerName);
                    if(layerTileServerSettings) {
                        layerTileServerSettings.IsSuppressed = true;
                    }
                }
            }
        }

        /**
         * Gets all known map layers.
         */
        getMapLayerSettings() : MapLayerSetting[]
        {
            var result: MapLayerSetting[] = [];

            $.each(this._MapLayerSettings, (idx, mapLayerSetting) => {
                result.push(mapLayerSetting);
            });

            return result;
        }

        //
        // State persistence methods
        saveState()
        {
            if(!this._ApplyingState && this._Map) {
                VRS.configStorage.save(this._PersistenceKey, this.createSettings());
            }
        }

        loadState() : MapLayerManager_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._PersistenceKey, {});
            return $.extend(this.createSettings(), savedSettings);
        }

        applyState(settings: MapLayerManager_SaveState)
        {
            if(!this._ApplyingState && this._Map) {
                try {
                    this._ApplyingState = true;

                    // Set opacities before making layers visible
                    $.each(this._MapLayerSettings, (idx, mapLayerSetting) => {
                        var opacityOverride = settings.opacityOverrides[mapLayerSetting.Name];
                        if(opacityOverride !== null && opacityOverride !== undefined && !isNaN(opacityOverride) && mapLayerSetting.OpacityOverride !== opacityOverride) {
                            mapLayerSetting.OpacityOverride = opacityOverride;
                        }
                    });

                    // Ensure all suppressed layers are flagged as suppressed before showing anything
                    $.each(this._SuppressedMapLayers, (idx, suppressedMapLayerName) => {
                        var mapLayer = VRS.arrayHelper.findFirst(this._MapLayerSettings, r => r.Name == suppressedMapLayerName);
                        if(mapLayer && !mapLayer.IsSuppressed) {
                            mapLayer.IsSuppressed = true;
                        }
                    });

                    // Ensure that layers are made visible in the same order that the user originally applied them
                    $.each(settings.visibleLayouts, (idx, visibleLayoutName) => {
                        var mapLayerSetting = VRS.arrayHelper.findFirst(this._MapLayerSettings, (r) => r.Name === visibleLayoutName);
                        if(mapLayerSetting && !mapLayerSetting.IsVisible) {
                            mapLayerSetting.show();
                        }
                    });

                    // I don't think I should hide layers that are already visible but aren't in the saved state... they were
                    // made visible by the user, I shouldn't override them.
                } finally {
                    this._ApplyingState = false;
                }
            }
        }

        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        private createSettings() : MapLayerManager_SaveState
        {
            var result: MapLayerManager_SaveState = {
                visibleLayouts: [],
                opacityOverrides: {}
            };

            this._MapLayerSettings.sort((lhs, rhs) => lhs.DisplayOrder - rhs.DisplayOrder);

            $.each(this._MapLayerSettings, (idx, mapLayerSetting) => {
                if(mapLayerSetting.IsVisible) {
                    result.visibleLayouts.push(mapLayerSetting.Name);
                }
                if(mapLayerSetting.OpacityOverride !== undefined) {
                    result.opacityOverrides[mapLayerSetting.Name] = mapLayerSetting.OpacityOverride;
                }
            });

            return result;
        }

        /**
         * Builds the _MapLayerSettings array of all known map layer settings, hooking new layers and
         * unhooking & destroying old ones.
         */
        private buildMapLayerSettings()
        {
            if(this._Map) {
                var newMapLayerSettings: MapLayerSetting[] = [];

                var tileServerSettings = VRS.serverConfig.get().TileServerLayers;
                $.each(this._CustomMapLayerSettings, (idx, customTileServerSettings) => {
                    tileServerSettings.push(customTileServerSettings);
                });

                var len = tileServerSettings.length;
                for(var i = 0;i < len;++i) {
                    var tileServerSetting = tileServerSettings[i];
                    var mapLayerSetting = VRS.arrayHelper.findFirst(this._MapLayerSettings, (r) => r.Name == tileServerSetting.Name);
                    if(!mapLayerSetting) {
                        mapLayerSetting = new MapLayerSetting(this._Map, tileServerSetting);
                        var hookHandles: HookHandles = {
                            opacityOverrideChangedEventHandle:  mapLayerSetting.hookOpacityOverrideChanged(this.mapLayer_opacityChanged, this),
                            visibilityChangedEventHandle:       mapLayerSetting.hookVisibilityChanged(this.mapLayer_visibilityChanged, this)
                        };
                        this._HookHandles[mapLayerSetting.Name] = hookHandles;
                    }
                    newMapLayerSettings.push(mapLayerSetting);
                }

                $.each(VRS.arrayHelper.except(this._MapLayerSettings, newMapLayerSettings, (lhs, rhs) => lhs.Name === rhs.Name), (idx, retiredMapLayer) => {
                    var hookHandles = this._HookHandles[retiredMapLayer.Name];
                    if(hookHandles) {
                        retiredMapLayer.unhook(hookHandles.opacityOverrideChangedEventHandle);
                        retiredMapLayer.unhook(hookHandles.visibilityChangedEventHandle);
                        delete this._HookHandles[retiredMapLayer.Name];
                    }

                    retiredMapLayer.hide();
                });

                this._MapLayerSettings = newMapLayerSettings;
            }
        }

        /**
         * Called when the server configuration changes.
         */
        private configurationChanged()
        {
            this.buildMapLayerSettings();
            this.loadAndApplyState();
        }

        /**
         * Called when a map layer changes its opacity override.
         * @param mapLayer
         */
        private mapLayer_opacityChanged(mapLayer: MapLayerSetting)
        {
            this.saveState();
        }

        /**
         * Called when a map layer changes its visibility.
         * @param mapLayer
         */
        private mapLayer_visibilityChanged(mapLayer: MapLayerSetting)
        {
            this.saveState();
        }
    }

    export var mapLayerManager = new MapLayerManager();
}
