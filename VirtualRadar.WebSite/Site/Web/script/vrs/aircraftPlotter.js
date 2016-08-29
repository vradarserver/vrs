var VRS;
(function (VRS) {
    var AircraftMarker = (function () {
        function AircraftMarker(settings) {
            this._Settings = $.extend({
                folder: 'images/web-markers',
                normalFileName: null,
                selectedFileName: settings.normalFileName || null,
                embeddedSvg: null,
                size: { width: 35, height: 35 },
                isAircraft: true,
                canRotate: true,
                isPre22Icon: false
            }, settings);
        }
        AircraftMarker.prototype.getFolder = function () {
            return this._Settings.folder;
        };
        AircraftMarker.prototype.setFolder = function (value) {
            this._Settings.folder = value;
        };
        AircraftMarker.prototype.getNormalFileName = function () {
            return this._Settings.normalFileName;
        };
        AircraftMarker.prototype.setNormalFileName = function (value) {
            this._Settings.normalFileName = value;
        };
        AircraftMarker.prototype.getSelectedFileName = function () {
            return this._Settings.selectedFileName;
        };
        AircraftMarker.prototype.setSelectedFileName = function (value) {
            this._Settings.selectedFileName = value;
        };
        AircraftMarker.prototype.getSize = function () {
            return this._Settings.size;
        };
        AircraftMarker.prototype.setSize = function (value) {
            this._Settings.size = value;
        };
        AircraftMarker.prototype.getIsAircraft = function () {
            return this._Settings.isAircraft;
        };
        AircraftMarker.prototype.setIsAircraft = function (value) {
            this._Settings.isAircraft = value;
        };
        AircraftMarker.prototype.getCanRotate = function () {
            return this._Settings.canRotate;
        };
        AircraftMarker.prototype.setCanRotate = function (value) {
            this._Settings.canRotate = value;
        };
        AircraftMarker.prototype.getIsPre22Icon = function () {
            return this._Settings.isPre22Icon;
        };
        AircraftMarker.prototype.setIsPre22Icon = function (value) {
            this._Settings.isPre22Icon = value;
        };
        AircraftMarker.prototype.getMatches = function () {
            return this._Settings.matches;
        };
        AircraftMarker.prototype.setMatches = function (value) {
            this._Settings.matches = value;
        };
        AircraftMarker.prototype.getEmbeddedSvg = function () {
            return this._Settings.embeddedSvg;
        };
        AircraftMarker.prototype.setEmbeddedSvg = function (value) {
            this._Settings.embeddedSvg = value;
        };
        AircraftMarker.prototype.matchesAircraft = function (aircraft) {
            return this._Settings.matches ? this._Settings.matches(aircraft) : false;
        };
        AircraftMarker.prototype.useEmbeddedSvg = function () {
            return VRS.globalOptions.aircraftMarkerUseSvg && this._Settings.embeddedSvg;
        };
        AircraftMarker.prototype.getSvgFillColour = function (aircraft, isSelected) {
            return isSelected ? VRS.globalOptions.svgAircraftMarkerSelectedFill : VRS.globalOptions.svgAircraftMarkerNormalFill;
        };
        return AircraftMarker;
    }());
    VRS.AircraftMarker = AircraftMarker;
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftMarkerPinTextWidth = VRS.globalOptions.aircraftMarkerPinTextWidth || 68;
    VRS.globalOptions.aircraftMarkerPinTextLineHeight = VRS.globalOptions.aircraftMarkerPinTextLineHeight || 12;
    VRS.globalOptions.aircraftMarkerRotate = VRS.globalOptions.aircraftMarkerRotate !== undefined ? VRS.globalOptions.aircraftMarkerRotate : true;
    VRS.globalOptions.aircraftMarkerRotationGranularity = VRS.globalOptions.aircraftMarkerRotationGranularity || 5;
    VRS.globalOptions.aircraftMarkerAllowAltitudeStalk = VRS.globalOptions.aircraftMarkerAllowAltitudeStalk !== undefined ? VRS.globalOptions.aircraftMarkerAllowAltitudeStalk : true;
    VRS.globalOptions.aircraftMarkerShowAltitudeStalk = VRS.globalOptions.aircraftMarkerShowAltitudeStalk !== undefined ? VRS.globalOptions.aircraftMarkerShowAltitudeStalk : true;
    VRS.globalOptions.aircraftMarkerAllowPinText = VRS.globalOptions.aircraftMarkerAllowPinText !== undefined ? VRS.globalOptions.aircraftMarkerAllowPinText : true;
    VRS.globalOptions.aircraftMarkerDefaultPinTexts = VRS.globalOptions.aircraftMarkerDefaultPinTexts ||
        [
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.Altitude
        ];
    VRS.globalOptions.aircraftMarkerPinTextLines = VRS.globalOptions.aircraftMarkerPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerPinTextLines : 3;
    VRS.globalOptions.aircraftMarkerMaximumPinTextLines = VRS.globalOptions.aircraftMarkerMaximumPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerMaximumPinTextLines : 6;
    VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines = VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines : false;
    VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed = VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed !== undefined ? VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed : true;
    VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel = VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel || 7;
    VRS.globalOptions.aircraftMarkerTrailColourNormal = VRS.globalOptions.aircraftMarkerTrailColourNormal || '#000040';
    VRS.globalOptions.aircraftMarkerTrailColourSelected = VRS.globalOptions.aircraftMarkerTrailColourSelected || '#202080';
    VRS.globalOptions.aircraftMarkerTrailWidthNormal = VRS.globalOptions.aircraftMarkerTrailWidthNormal || 2;
    VRS.globalOptions.aircraftMarkerTrailWidthSelected = VRS.globalOptions.aircraftMarkerTrailWidthSelected || 3;
    VRS.globalOptions.aircraftMarkerTrailDisplay = VRS.globalOptions.aircraftMarkerTrailDisplay || VRS.TrailDisplay.SelectedOnly;
    VRS.globalOptions.aircraftMarkerTrailType = VRS.globalOptions.aircraftMarkerTrailType || VRS.TrailType.Full;
    VRS.globalOptions.aircraftMarkerShowTooltip = VRS.globalOptions.aircraftMarkerShowTooltip !== undefined ? VRS.globalOptions.aircraftMarkerShowTooltip : true;
    VRS.globalOptions.aircraftMarkerMovingMapOn = VRS.globalOptions.aircraftMarkerMovingMapOn !== undefined ? VRS.globalOptions.aircraftMarkerMovingMapOn : false;
    VRS.globalOptions.aircraftMarkerSuppressTextOnImages = VRS.globalOptions.aircraftMarkerSuppressTextOnImages !== undefined ? VRS.globalOptions.aircraftMarkerSuppressTextOnImages : undefined;
    VRS.globalOptions.aircraftMarkerAllowRangeCircles = VRS.globalOptions.aircraftMarkerAllowRangeCircles !== undefined ? VRS.globalOptions.aircraftMarkerAllowRangeCircles : true;
    VRS.globalOptions.aircraftMarkerShowRangeCircles = VRS.globalOptions.aircraftMarkerShowRangeCircles !== undefined ? VRS.globalOptions.aircraftMarkerShowRangeCircles : false;
    VRS.globalOptions.aircraftMarkerRangeCircleInterval = VRS.globalOptions.aircraftMarkerRangeCircleInterval || 20;
    VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit = VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit || VRS.Distance.StatuteMile;
    VRS.globalOptions.aircraftMarkerRangeCircleCount = VRS.globalOptions.aircraftMarkerRangeCircleCount || 6;
    VRS.globalOptions.aircraftMarkerRangeCircleOddColour = VRS.globalOptions.aircraftMarkerRangeCircleOddColour || '#333333';
    VRS.globalOptions.aircraftMarkerRangeCircleEvenColour = VRS.globalOptions.aircraftMarkerRangeCircleEvenColour || '#111111';
    VRS.globalOptions.aircraftMarkerRangeCircleOddWeight = VRS.globalOptions.aircraftMarkerRangeCircleOddWeight || 1;
    VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight = VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight || 2;
    VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles = VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles || 9;
    VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval = VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval || 100;
    VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight || 4;
    VRS.globalOptions.aircraftMarkerAltitudeTrailLow = VRS.globalOptions.aircraftMarkerAltitudeTrailLow !== undefined ? VRS.globalOptions.aircraftMarkerAltitudeTrailLow : 300;
    VRS.globalOptions.aircraftMarkerAltitudeTrailHigh = VRS.globalOptions.aircraftMarkerAltitudeTrailHigh || 45000;
    VRS.globalOptions.aircraftMarkerSpeedTrailLow = VRS.globalOptions.aircraftMarkerSpeedTrailLow !== undefined ? VRS.globalOptions.aircraftMarkerSpeedTrailLow : 10;
    VRS.globalOptions.aircraftMarkerSpeedTrailHigh = VRS.globalOptions.aircraftMarkerSpeedTrailHigh || 660;
    VRS.globalOptions.aircraftMarkerAlwaysPlotSelected = VRS.globalOptions.aircraftMarkerAlwaysPlotSelected !== undefined ? VRS.globalOptions.aircraftMarkerAlwaysPlotSelected : true;
    VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel = VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel != undefined ? VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel : 13;
    VRS.globalOptions.aircraftMarkerShowNonAircraftTrails = VRS.globalOptions.aircraftMarkerShowNonAircraftTrails !== undefined ? VRS.globalOptions.aircraftMarkerShowNonAircraftTrails : false;
    VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons = VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons !== undefined ? VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons : false;
    VRS.globalOptions.aircraftMarkerClustererEnabled = VRS.globalOptions.aircraftMarkerClustererEnabled !== false;
    VRS.globalOptions.aircraftMarkerClustererMaxZoom = VRS.globalOptions.aircraftMarkerClustererMaxZoom || 5;
    VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize = VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize || 1;
    VRS.globalOptions.aircraftMarkerClustererUserCanConfigure = VRS.globalOptions.aircraftMarkerClustererUserCanConfigure !== false;
    VRS.globalOptions.aircraftMarkerUseSvg = VRS.globalOptions.aircraftMarkerUseSvg !== false;
    VRS.globalOptions.aircraftMarkers = VRS.globalOptions.aircraftMarkers || [
        new VRS.AircraftMarker({
            normalFileName: 'GroundVehicle.png',
            selectedFileName: 'GroundVehicle.png',
            size: { width: 26, height: 24 },
            isAircraft: false,
            isPre22Icon: true,
            matches: function (aircraft) { return aircraft.species.val === VRS.Species.GroundVehicle; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Tower.png',
            selectedFileName: 'Tower.png',
            size: { width: 20, height: 20 },
            isAircraft: false,
            isPre22Icon: true,
            canRotate: false,
            matches: function (aircraft) { return aircraft.species.val === VRS.Species.Tower; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Helicopter.png',
            selectedFileName: 'Helicopter-Selected.png',
            size: { width: 32, height: 32 },
            matches: function (aircraft) { return aircraft.species.val === VRS.Species.Helicopter; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Type-GLID.png',
            selectedFileName: 'Type-GLID-Selected.png',
            size: { width: 60, height: 60 },
            matches: function (aircraft) { return aircraft.modelIcao.val === 'GLID'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Type-A380.png',
            selectedFileName: 'Type-A380-Selected.png',
            size: { width: 60, height: 60 },
            matches: function (aircraft) { return aircraft.modelIcao.val === 'A388'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Type-A340.png',
            selectedFileName: 'Type-A340-Selected.png',
            size: { width: 60, height: 60 },
            matches: function (aircraft) { return aircraft.modelIcao.val && (aircraft.modelIcao.val === 'E6' || (aircraft.modelIcao.val.length === 4 && aircraft.modelIcao.val.substring(0, 3) === 'A34')); }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Light-1-Prop.png',
            selectedFileName: 'WTC-Light-1-Prop-Selected.png',
            size: { width: 32, height: 32 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light && aircraft.engineType.val !== VRS.EngineType.Jet && aircraft.countEngines.val === '1'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Light-2-Prop.png',
            selectedFileName: 'WTC-Light-2-Prop-Selected.png',
            size: { width: 36, height: 36 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light && aircraft.engineType.val !== VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Type-GLFx.png',
            selectedFileName: 'Type-GLFx-Selected.png',
            size: { width: 40, height: 40 },
            matches: function (aircraft) {
                return aircraft.engineType.val === VRS.EngineType.Jet &&
                    (aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light ||
                        (aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.enginePlacement.val === VRS.EnginePlacement.AftMounted));
            }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Medium-4-Jet.png',
            selectedFileName: 'WTC-Medium-4-Jet-Selected.png',
            size: { width: 40, height: 40 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.countEngines.val === '4' && aircraft.engineType.val === VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Medium-2-Jet.png',
            selectedFileName: 'WTC-Medium-2-Jet-Selected.png',
            embeddedSvg: VRS.EmbeddedSvgs.Marker_Medium2Jet,
            size: { width: 40, height: 40 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.countEngines.val !== '4' && aircraft.engineType.val === VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Medium-2-Turbo.png',
            selectedFileName: 'WTC-Medium-2-Turbo-Selected.png',
            size: { width: 40, height: 40 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.countEngines.val !== '4'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Heavy-4-Jet.png',
            selectedFileName: 'WTC-Heavy-4-Jet-Selected.png',
            size: { width: 60, height: 60 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Heavy && aircraft.countEngines.val === '4'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'WTC-Heavy-2-Jet.png',
            selectedFileName: 'WTC-Heavy-2-Jet-Selected.png',
            size: { width: 57, height: 57 },
            matches: function (aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Heavy && aircraft.countEngines.val !== '4'; }
        }),
        new VRS.AircraftMarker({
            normalFileName: '4-TurboProp.png',
            selectedFileName: '4-TurboPropSelected.png',
            size: { width: 40, height: 40 },
            matches: function (aircraft) { return aircraft.countEngines.val === '4' && aircraft.engineType.val === VRS.EngineType.Turbo; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Airplane.png',
            selectedFileName: 'AirplaneSelected.png',
            size: { width: 35, height: 35 },
            isPre22Icon: true,
            matches: function () { return true; }
        })
    ];
    var PlottedDetail = (function () {
        function PlottedDetail(aircraft) {
            this.aircraft = aircraft;
            this.mapPolylines = [];
            this.nextPolylineId = 0;
            this.pinTexts = [];
            this.polylinePathUpdateCounter = -1;
            this._Id = aircraft.id;
        }
        Object.defineProperty(PlottedDetail.prototype, "id", {
            get: function () {
                return this._Id;
            },
            enumerable: true,
            configurable: true
        });
        return PlottedDetail;
    }());
    var AircraftPlotterOptions = (function () {
        function AircraftPlotterOptions(settings) {
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftPlotterOptions'
            });
            this._Events = {
                propertyChanged: 'propertyChanged',
                rangeCirclePropertyChanged: 'rangeCirclePropertyChanged'
            };
            this._SuppressEvents = false;
            this._PinTexts = [];
            this.getName = function () {
                return _this._Settings.name;
            };
            this.getMap = function () {
                return _this._Settings.map;
            };
            this.setMap = function (map) {
                _this._Settings.map = map;
            };
            this.getShowAltitudeStalk = function () {
                return _this._Settings.showAltitudeStalk;
            };
            this.setShowAltitudeStalk = function (value) {
                if (_this._Settings.showAltitudeStalk !== value) {
                    _this._Settings.showAltitudeStalk = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getSuppressAltitudeStalkWhenZoomedOut = function () {
                return _this._Settings.suppressAltitudeStalkWhenZoomed;
            };
            this.setSuppressAltitudeStalkWhenZoomedOut = function (value) {
                if (_this._Settings.suppressAltitudeStalkWhenZoomed !== value) {
                    _this._Settings.suppressAltitudeStalkWhenZoomed = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getShowPinText = function () {
                return _this._Settings.showPinText;
            };
            this.setShowPinText = function (value) {
                if (_this._Settings.showPinText !== value) {
                    _this._Settings.showPinText = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getPinTexts = function () {
                return _this._PinTexts;
            };
            this.getPinText = function (index) {
                return index >= _this._PinTexts.length ? VRS.RenderProperty.None : _this._PinTexts[index];
            };
            this.setPinText = function (index, value) {
                if (index <= VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                    if (!VRS.renderPropertyHandlers[value] || !VRS.renderPropertyHandlers[value].isSurfaceSupported(VRS.RenderSurface.Marker)) {
                        value = VRS.RenderProperty.None;
                    }
                    if (_this.getPinText[index] !== value) {
                        if (index < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                            while (_this._PinTexts.length <= index) {
                                _this._PinTexts.push(_this._PinTexts.length < _this._Settings.pinTexts.length ? _this._Settings.pinTexts[_this._PinTexts.length] : VRS.RenderProperty.None);
                            }
                            _this._PinTexts[index] = value;
                            _this.raisePropertyChanged();
                        }
                    }
                }
            };
            this.getPinTextLines = function () {
                return _this._Settings.pinTextLines;
            };
            this.setPinTextLines = function (value) {
                if (value !== _this._Settings.pinTextLines) {
                    _this._Settings.pinTextLines = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getHideEmptyPinTextLines = function () {
                return _this._Settings.hideEmptyPinTextLines;
            };
            this.setHideEmptyPinTextLines = function (value) {
                if (value !== _this._Settings.hideEmptyPinTextLines) {
                    _this._Settings.hideEmptyPinTextLines = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getTrailDisplay = function () {
                return _this._Settings.trailDisplay;
            };
            this.setTrailDisplay = function (value) {
                if (value !== _this._Settings.trailDisplay) {
                    _this._Settings.trailDisplay = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getTrailType = function () {
                return _this._Settings.trailType;
            };
            this.setTrailType = function (value) {
                if (value !== _this._Settings.trailType) {
                    _this._Settings.trailType = value;
                }
            };
            this.getShowRangeCircles = function () {
                return _this._Settings.showRangeCircles;
            };
            this.setShowRangeCircles = function (value) {
                if (value !== _this._Settings.showRangeCircles) {
                    _this._Settings.showRangeCircles = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleInterval = function () {
                return _this._Settings.rangeCircleInterval;
            };
            this.setRangeCircleInterval = function (value) {
                if (value !== _this._Settings.rangeCircleInterval) {
                    _this._Settings.rangeCircleInterval = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleDistanceUnit = function () {
                return _this._Settings.rangeCircleDistanceUnit;
            };
            this.setRangeCircleDistanceUnit = function (value) {
                if (value !== _this._Settings.rangeCircleDistanceUnit) {
                    _this._Settings.rangeCircleDistanceUnit = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleCount = function () {
                return _this._Settings.rangeCircleCount;
            };
            this.setRangeCircleCount = function (value) {
                if (value !== _this._Settings.rangeCircleCount) {
                    _this._Settings.rangeCircleCount = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleOddColour = function () {
                return _this._Settings.rangeCircleOddColour;
            };
            this.setRangeCircleOddColour = function (value) {
                if (value !== _this._Settings.rangeCircleOddColour) {
                    _this._Settings.rangeCircleOddColour = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleOddWeight = function () {
                return _this._Settings.rangeCircleOddWeight;
            };
            this.setRangeCircleOddWeight = function (value) {
                if (value !== _this._Settings.rangeCircleOddWeight) {
                    _this._Settings.rangeCircleOddWeight = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleEvenColour = function () {
                return _this._Settings.rangeCircleEvenColour;
            };
            this.setRangeCircleEvenColour = function (value) {
                if (value !== _this._Settings.rangeCircleEvenColour) {
                    _this._Settings.rangeCircleEvenColour = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getRangeCircleEvenWeight = function () {
                return _this._Settings.rangeCircleEvenWeight;
            };
            this.setRangeCircleEvenWeight = function (value) {
                if (value !== _this._Settings.rangeCircleEvenWeight) {
                    _this._Settings.rangeCircleEvenWeight = value;
                    _this.raiseRangeCirclePropertyChanged();
                }
            };
            this.getOnlyUsePre22Icons = function () {
                return _this._Settings.onlyUsePre22Icons;
            };
            this.setOnlyUsePre22Icons = function (value) {
                if (_this._Settings.onlyUsePre22Icons !== value) {
                    _this._Settings.onlyUsePre22Icons = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getAircraftMarkerClustererMaxZoom = function () {
                return _this._Settings.aircraftMarkerClustererMaxZoom;
            };
            this.setAircraftMarkerClustererMaxZoom = function (value) {
                if (_this._Settings.aircraftMarkerClustererMaxZoom !== value) {
                    _this._Settings.aircraftMarkerClustererMaxZoom = value;
                    _this.raisePropertyChanged();
                }
            };
            this.getCanSetAircraftMarkerClustererMaxZoomFromMap = function () {
                return !!_this._Settings.map;
            };
            this.hookPropertyChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.propertyChanged, callback, forceThis);
            };
            this.hookRangeCirclePropertyChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.rangeCirclePropertyChanged, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                var result = $.extend(_this.createSettings(), savedSettings);
                if (result.showAltitudeStalk && !VRS.globalOptions.aircraftMarkerAllowAltitudeStalk)
                    result.showAltitudeStalk = false;
                if (result.showPinText && !VRS.globalOptions.aircraftMarkerAllowPinText)
                    result.showPinText = false;
                if (!result.onlyUsePre22Icons && VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons)
                    result.onlyUsePre22Icons = true;
                if (result.rangeCircleCount > VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles)
                    result.rangeCircleCount = VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles;
                if (result.rangeCircleInterval > VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval)
                    result.rangeCircleInterval = VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval;
                if (result.rangeCircleEvenWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight)
                    result.rangeCircleEvenWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;
                if (result.rangeCircleOddWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight)
                    result.rangeCircleOddWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;
                result.pinTexts = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.pinTexts, [VRS.RenderSurface.Marker], VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
                while (result.pinTexts.length < _this._Settings.pinTexts.length && result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                    result.pinTexts.push(_this._Settings.pinTexts[result.pinTexts.length]);
                }
                while (result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                    result.pinTexts.push(VRS.RenderProperty.None);
                }
                return result;
            };
            this.applyState = function (settings) {
                var suppressEvents = _this._SuppressEvents;
                _this._SuppressEvents = true;
                _this.setSuppressAltitudeStalkWhenZoomedOut(settings.suppressAltitudeStalkWhenZoomedOut);
                _this.setShowAltitudeStalk(settings.showAltitudeStalk);
                _this.setShowPinText(settings.showPinText);
                for (var i = 0; i < settings.pinTexts.length; ++i) {
                    _this.setPinText(i, settings.pinTexts[i]);
                }
                _this.setPinTextLines(settings.pinTextLines);
                _this.setHideEmptyPinTextLines(settings.hideEmptyPinTextLines);
                _this.setTrailType(settings.trailType);
                _this.setTrailDisplay(settings.trailDisplay);
                _this.setShowRangeCircles(settings.showRangeCircles);
                _this.setRangeCircleCount(settings.rangeCircleCount);
                _this.setRangeCircleInterval(settings.rangeCircleInterval);
                _this.setRangeCircleDistanceUnit(settings.rangeCircleDistanceUnit);
                _this.setRangeCircleOddColour(settings.rangeCircleOddColour);
                _this.setRangeCircleOddWeight(settings.rangeCircleOddWeight);
                _this.setRangeCircleEvenColour(settings.rangeCircleEvenColour);
                _this.setRangeCircleEvenWeight(settings.rangeCircleEvenWeight);
                _this.setOnlyUsePre22Icons(settings.onlyUsePre22Icons);
                _this.setAircraftMarkerClustererMaxZoom(settings.aircraftMarkerClustererMaxZoom);
                _this._SuppressEvents = suppressEvents;
                _this.raisePropertyChanged();
                _this.raiseRangeCirclePropertyChanged();
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var result = [];
                var displayPane = new VRS.OptionPane({
                    name: 'vrsAircraftPlotterOptionsDisplayPane_' + _this.getName(),
                    titleKey: 'PaneAircraftDisplay',
                    displayOrder: displayOrder
                });
                result.push(displayPane);
                if (VRS.globalOptions.aircraftMarkerAllowAltitudeStalk) {
                    displayPane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showAltitudeStalk',
                        labelKey: 'ShowAltitudeStalk',
                        getValue: _this.getShowAltitudeStalk,
                        setValue: _this.setShowAltitudeStalk,
                        saveState: _this.saveState
                    }));
                    displayPane.addField(new VRS.OptionFieldCheckBox({
                        name: 'suppressAltitudeStalkWhenZoomedOut',
                        labelKey: 'SuppressAltitudeStalkWhenZoomedOut',
                        getValue: _this.getSuppressAltitudeStalkWhenZoomedOut,
                        setValue: _this.setSuppressAltitudeStalkWhenZoomedOut,
                        saveState: _this.saveState
                    }));
                }
                if (!VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons) {
                    displayPane.addField(new VRS.OptionFieldCheckBox({
                        name: 'onlyUsePre22Icons',
                        labelKey: 'OnlyUsePre22Icons',
                        getValue: _this.getOnlyUsePre22Icons,
                        setValue: _this.setOnlyUsePre22Icons,
                        saveState: _this.saveState
                    }));
                }
                var showPinTextOptions = VRS.globalOptions.aircraftMarkerAllowPinText;
                if (showPinTextOptions && VRS.serverConfig)
                    showPinTextOptions = VRS.serverConfig.pinTextEnabled();
                if (showPinTextOptions) {
                    var handlers = VRS.renderPropertyHelper.getHandlersForSurface(VRS.RenderSurface.Marker);
                    VRS.renderPropertyHelper.sortHandlers(handlers, true);
                    var values = VRS.renderPropertyHelper.createValueTextListForHandlers(handlers);
                    var pinTextLines = VRS.globalOptions.aircraftMarkerMaximumPinTextLines;
                    var buildContentFieldName = function (idx) {
                        return 'pinText' + idx;
                    };
                    displayPane.addField(new VRS.OptionFieldNumeric({
                        name: 'pinTextLines',
                        labelKey: 'PinTextLines',
                        getValue: _this.getPinTextLines,
                        setValue: _this.setPinTextLines,
                        min: 0,
                        max: pinTextLines,
                        inputWidth: VRS.InputWidth.OneChar,
                        saveState: function () {
                            _this.saveState();
                            for (var contentIdx = 0; contentIdx < pinTextLines; ++contentIdx) {
                                var contentField = displayPane.getFieldByName(buildContentFieldName(contentIdx));
                                if (contentField)
                                    contentField.raiseRefreshFieldVisibility();
                            }
                        }
                    }));
                    var addPinTextContentField = function (idx) {
                        displayPane.addField(new VRS.OptionFieldComboBox({
                            name: buildContentFieldName(idx),
                            labelKey: function () { return VRS.stringUtility.format(VRS.$$.PinTextNumber, idx + 1); },
                            getValue: function () { return _this.getPinText(idx); },
                            setValue: function (value) { return _this.setPinText(idx, value); },
                            saveState: _this.saveState,
                            visible: function () { return idx < _this.getPinTextLines(); },
                            values: values
                        }));
                    };
                    for (var lineIdx = 0; lineIdx < pinTextLines; ++lineIdx) {
                        addPinTextContentField(lineIdx);
                    }
                    displayPane.addField(new VRS.OptionFieldCheckBox({
                        name: 'hideEmptyPinTextLines',
                        labelKey: 'HideEmptyPinTextLines',
                        getValue: _this.getHideEmptyPinTextLines,
                        setValue: _this.setHideEmptyPinTextLines,
                        saveState: _this.saveState
                    }));
                    if (VRS.globalOptions.aircraftMarkerClustererUserCanConfigure && VRS.globalOptions.aircraftMarkerClustererEnabled && _this.getCanSetAircraftMarkerClustererMaxZoomFromMap()) {
                        displayPane.addField(new VRS.OptionFieldButton({
                            name: 'aircraftMarkerClustererSetMaxZoom',
                            saveState: function () {
                                _this.setAircraftMarkerClusterMaxZoomFromMap();
                                _this.saveState();
                            },
                            keepWithNext: true,
                            labelKey: 'SetClustererZoomLevel'
                        }));
                        displayPane.addField(new VRS.OptionFieldButton({
                            name: 'resetAircraftMarkerClustererMaxZoom',
                            saveState: function () {
                                _this.setAircraftMarkerClustererMaxZoom(VRS.globalOptions.aircraftMarkerClustererMaxZoom);
                                _this.saveState();
                            },
                            labelKey: 'ResetClustererZoomLevel'
                        }));
                    }
                }
                if (!VRS.globalOptions.suppressTrails) {
                    var trailsPane = new VRS.OptionPane({
                        name: 'vrsAircraftPlotterTrailsPane' + _this.getName(),
                        titleKey: 'PaneAircraftTrails',
                        displayOrder: displayOrder + 1,
                        fields: [
                            new VRS.OptionFieldRadioButton({
                                name: 'trailDisplay',
                                getValue: _this.getTrailDisplay,
                                setValue: _this.setTrailDisplay,
                                saveState: _this.saveState,
                                values: [
                                    new VRS.ValueText({ value: VRS.TrailDisplay.None, textKey: 'DoNotShow' }),
                                    new VRS.ValueText({ value: VRS.TrailDisplay.SelectedOnly, textKey: 'ShowForSelectedOnly' }),
                                    new VRS.ValueText({ value: VRS.TrailDisplay.AllAircraft, textKey: 'ShowForAllAircraft' })
                                ]
                            }),
                            new VRS.OptionFieldRadioButton({
                                name: 'trailType',
                                getValue: function () {
                                    switch (_this.getTrailType()) {
                                        case VRS.TrailType.FullAltitude:
                                        case VRS.TrailType.ShortAltitude:
                                            return VRS.TrailType.FullAltitude;
                                        case VRS.TrailType.FullSpeed:
                                        case VRS.TrailType.ShortSpeed:
                                            return VRS.TrailType.FullSpeed;
                                    }
                                    return VRS.TrailType.Full;
                                },
                                setValue: function (value) {
                                    switch (_this.getTrailType()) {
                                        case VRS.TrailType.Full:
                                        case VRS.TrailType.FullAltitude:
                                        case VRS.TrailType.FullSpeed:
                                            _this.setTrailType(value);
                                            break;
                                        default:
                                            switch (value) {
                                                case VRS.TrailType.Full:
                                                    _this.setTrailType(VRS.TrailType.Short);
                                                    break;
                                                case VRS.TrailType.FullAltitude:
                                                    _this.setTrailType(VRS.TrailType.ShortAltitude);
                                                    break;
                                                case VRS.TrailType.FullSpeed:
                                                    _this.setTrailType(VRS.TrailType.ShortSpeed);
                                                    break;
                                            }
                                    }
                                },
                                saveState: _this.saveState,
                                values: [
                                    new VRS.ValueText({ value: VRS.TrailType.Full, textKey: 'JustPositions' }),
                                    new VRS.ValueText({ value: VRS.TrailType.FullAltitude, textKey: 'PositionAndAltitude' }),
                                    new VRS.ValueText({ value: VRS.TrailType.FullSpeed, textKey: 'PositionAndSpeed' })
                                ]
                            }),
                            new VRS.OptionFieldCheckBox({
                                name: 'showShortTrails',
                                labelKey: 'ShowShortTrails',
                                getValue: function () {
                                    switch (_this.getTrailType()) {
                                        case VRS.TrailType.Full:
                                        case VRS.TrailType.FullAltitude:
                                        case VRS.TrailType.FullSpeed:
                                            return false;
                                        default:
                                            return true;
                                    }
                                },
                                setValue: function (value) {
                                    switch (_this.getTrailType()) {
                                        case VRS.TrailType.Full:
                                        case VRS.TrailType.Short:
                                            _this.setTrailType(value ? VRS.TrailType.Short : VRS.TrailType.Full);
                                            break;
                                        case VRS.TrailType.FullAltitude:
                                        case VRS.TrailType.ShortAltitude:
                                            _this.setTrailType(value ? VRS.TrailType.ShortAltitude : VRS.TrailType.FullAltitude);
                                            break;
                                        case VRS.TrailType.FullSpeed:
                                        case VRS.TrailType.ShortSpeed:
                                            _this.setTrailType(value ? VRS.TrailType.ShortSpeed : VRS.TrailType.FullSpeed);
                                            break;
                                    }
                                },
                                saveState: _this.saveState
                            })
                        ]
                    });
                    result.push(trailsPane);
                }
                return result;
            };
            this.createOptionPaneForRangeCircles = function (displayOrder) {
                return new VRS.OptionPane({
                    name: 'rangeCircle',
                    titleKey: 'PaneRangeCircles',
                    displayOrder: displayOrder,
                    fields: [
                        new VRS.OptionFieldCheckBox({
                            name: 'show',
                            labelKey: 'ShowRangeCircles',
                            getValue: _this.getShowRangeCircles,
                            setValue: _this.setShowRangeCircles,
                            saveState: _this.saveState
                        }),
                        new VRS.OptionFieldNumeric({
                            name: 'count',
                            labelKey: 'Quantity',
                            getValue: _this.getRangeCircleCount,
                            setValue: _this.setRangeCircleCount,
                            saveState: _this.saveState,
                            inputWidth: VRS.InputWidth.ThreeChar,
                            min: 1,
                            max: VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles
                        }),
                        new VRS.OptionFieldNumeric({
                            name: 'interval',
                            labelKey: 'Distance',
                            getValue: _this.getRangeCircleInterval,
                            setValue: _this.setRangeCircleInterval,
                            saveState: _this.saveState,
                            inputWidth: VRS.InputWidth.ThreeChar,
                            min: 5,
                            max: VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval,
                            step: 5,
                            keepWithNext: true
                        }),
                        new VRS.OptionFieldComboBox({
                            name: 'distanceUnit',
                            getValue: _this.getRangeCircleDistanceUnit,
                            setValue: _this.setRangeCircleDistanceUnit,
                            saveState: _this.saveState,
                            values: VRS.UnitDisplayPreferences.getDistanceUnitValues()
                        }),
                        new VRS.OptionFieldColour({
                            name: 'oddColour',
                            labelKey: 'RangeCircleOddColour',
                            getValue: _this.getRangeCircleOddColour,
                            setValue: _this.setRangeCircleOddColour,
                            saveState: _this.saveState,
                            keepWithNext: true
                        }),
                        new VRS.OptionFieldNumeric({
                            name: 'oddWidth',
                            getValue: _this.getRangeCircleOddWeight,
                            setValue: _this.setRangeCircleOddWeight,
                            saveState: _this.saveState,
                            inputWidth: VRS.InputWidth.OneChar,
                            min: 1,
                            max: VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight,
                            keepWithNext: true
                        }),
                        new VRS.OptionFieldLabel({
                            name: 'oddPixels',
                            labelKey: 'Pixels'
                        }),
                        new VRS.OptionFieldColour({
                            name: 'evenColour',
                            labelKey: 'RangeCircleEvenColour',
                            getValue: _this.getRangeCircleEvenColour,
                            setValue: _this.setRangeCircleEvenColour,
                            saveState: _this.saveState,
                            keepWithNext: true
                        }),
                        new VRS.OptionFieldNumeric({
                            name: 'evenWidth',
                            getValue: _this.getRangeCircleEvenWeight,
                            setValue: _this.setRangeCircleEvenWeight,
                            saveState: _this.saveState,
                            inputWidth: VRS.InputWidth.OneChar,
                            min: 1,
                            max: VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight,
                            keepWithNext: true
                        }),
                        new VRS.OptionFieldLabel({
                            name: 'evenPixels',
                            labelKey: 'Pixels'
                        })
                    ]
                });
            };
            this._Settings = $.extend({
                name: 'default',
                map: null,
                showAltitudeStalk: VRS.globalOptions.aircraftMarkerAllowAltitudeStalk && VRS.globalOptions.aircraftMarkerShowAltitudeStalk,
                suppressAltitudeStalkWhenZoomed: VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed,
                showPinText: VRS.globalOptions.aircraftMarkerAllowPinText,
                pinTexts: VRS.globalOptions.aircraftMarkerDefaultPinTexts,
                pinTextLines: VRS.globalOptions.aircraftMarkerPinTextLines,
                hideEmptyPinTextLines: VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines,
                trailDisplay: VRS.globalOptions.aircraftMarkerTrailDisplay,
                trailType: VRS.globalOptions.aircraftMarkerTrailType,
                showRangeCircles: VRS.globalOptions.aircraftMarkerShowRangeCircles,
                rangeCircleInterval: VRS.globalOptions.aircraftMarkerRangeCircleInterval,
                rangeCircleDistanceUnit: VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit,
                rangeCircleCount: VRS.globalOptions.aircraftMarkerRangeCircleCount,
                rangeCircleOddColour: VRS.globalOptions.aircraftMarkerRangeCircleOddColour,
                rangeCircleOddWeight: VRS.globalOptions.aircraftMarkerRangeCircleOddWeight,
                rangeCircleEvenColour: VRS.globalOptions.aircraftMarkerRangeCircleEvenColour,
                rangeCircleEvenWeight: VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight,
                onlyUsePre22Icons: VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons,
                aircraftMarkerClustererMaxZoom: VRS.globalOptions.aircraftMarkerClustererMaxZoom
            }, settings);
            $.each(this._Settings.pinTexts, function (idx, renderProperty) {
                _this.setPinText(idx, renderProperty);
            });
            for (var noPinTextIdx = this._Settings.pinTexts ? this._Settings.pinTexts.length : 0; noPinTextIdx < VRS.globalOptions.aircraftMarkerMaximumPinTextLines; ++noPinTextIdx) {
                this.setPinText(noPinTextIdx, VRS.RenderProperty.None);
            }
        }
        AircraftPlotterOptions.prototype.setAircraftMarkerClusterMaxZoomFromMap = function () {
            if (this.getCanSetAircraftMarkerClustererMaxZoomFromMap()) {
                this.setAircraftMarkerClustererMaxZoom(this._Settings.map.getZoom());
            }
        };
        AircraftPlotterOptions.prototype.raisePropertyChanged = function () {
            if (!this._SuppressEvents) {
                this._Dispatcher.raise(this._Events.propertyChanged);
            }
        };
        AircraftPlotterOptions.prototype.raiseRangeCirclePropertyChanged = function () {
            if (!this._SuppressEvents) {
                this._Dispatcher.raise(this._Events.rangeCirclePropertyChanged);
            }
        };
        AircraftPlotterOptions.prototype.persistenceKey = function () {
            return 'vrsAircraftPlotterOptions-' + this.getName();
        };
        AircraftPlotterOptions.prototype.createSettings = function () {
            return {
                showAltitudeStalk: this.getShowAltitudeStalk(),
                suppressAltitudeStalkWhenZoomedOut: this.getSuppressAltitudeStalkWhenZoomedOut(),
                showPinText: this.getShowPinText(),
                pinTexts: this.getPinTexts(),
                pinTextLines: this.getPinTextLines(),
                hideEmptyPinTextLines: this.getHideEmptyPinTextLines(),
                trailDisplay: this.getTrailDisplay(),
                trailType: this.getTrailType(),
                showRangeCircles: this.getShowRangeCircles(),
                rangeCircleInterval: this.getRangeCircleInterval(),
                rangeCircleDistanceUnit: this.getRangeCircleDistanceUnit(),
                rangeCircleCount: this.getRangeCircleCount(),
                rangeCircleOddColour: this.getRangeCircleOddColour(),
                rangeCircleOddWeight: this.getRangeCircleOddWeight(),
                rangeCircleEvenColour: this.getRangeCircleEvenColour(),
                rangeCircleEvenWeight: this.getRangeCircleEvenWeight(),
                onlyUsePre22Icons: this.getOnlyUsePre22Icons(),
                aircraftMarkerClustererMaxZoom: this.getAircraftMarkerClustererMaxZoom(),
            };
        };
        return AircraftPlotterOptions;
    }());
    VRS.AircraftPlotterOptions = AircraftPlotterOptions;
    var AircraftPlotter = (function () {
        function AircraftPlotter(settings) {
            var _this = this;
            this._Suspended = false;
            this._PlottedDetail = {};
            this._RangeCircleCentre = null;
            this._RangeCircleCircles = [];
            this._MovingMap = VRS.globalOptions.aircraftMarkerMovingMapOn;
            this._SvgGenerator = new VRS.SvgGenerator();
            settings = $.extend({
                name: 'default',
                aircraftMarkers: VRS.globalOptions.aircraftMarkers,
                pinTextMarkerWidth: VRS.globalOptions.aircraftMarkerPinTextWidth,
                pinTextLineHeight: VRS.globalOptions.aircraftMarkerPinTextLineHeight,
                allowRotation: VRS.globalOptions.aircraftMarkerRotate,
                rotationGranularity: VRS.globalOptions.aircraftMarkerRotationGranularity,
                suppressAltitudeStalkAboveZoom: VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel,
                normalTrailColour: VRS.globalOptions.aircraftMarkerTrailColourNormal,
                selectedTrailColour: VRS.globalOptions.aircraftMarkerTrailColourSelected,
                normalTrailWidth: VRS.globalOptions.aircraftMarkerTrailWidthNormal,
                selectedTrailWidth: VRS.globalOptions.aircraftMarkerTrailWidthSelected,
                showTooltips: VRS.globalOptions.aircraftMarkerShowTooltip,
                suppressTextOnImages: VRS.globalOptions.aircraftMarkerSuppressTextOnImages,
                allowRangeCircles: VRS.globalOptions.aircraftMarkerAllowRangeCircles,
                hideNonAircraftZoomLevel: VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel,
                showNonAircraftTrails: VRS.globalOptions.aircraftMarkerShowNonAircraftTrails
            }, settings);
            this._Settings = settings;
            this._Map = VRS.jQueryUIHelper.getMapPlugin(settings.map);
            if (VRS.globalOptions.aircraftMarkerClustererEnabled) {
                this._MapMarkerClusterer = this._Map.createMapMarkerClusterer({
                    maxZoom: settings.plotterOptions.getAircraftMarkerClustererMaxZoom(),
                    minimumClusterSize: VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize
                });
            }
            this._UnitDisplayPreferences = settings.unitDisplayPreferences || new VRS.UnitDisplayPreferences();
            this._SuppressTextOnImages = settings.suppressTextOnImages;
            this._GetSelectedAircraft = settings.getSelectedAircraft || function () {
                return settings.aircraftList ? settings.aircraftList.getSelectedAircraft() : null;
            };
            this._GetAircraft = settings.getAircraft || function () {
                return settings.aircraftList ? settings.aircraftList.getAircraft() : new VRS.AircraftCollection();
            };
            this.configureSuppressTextOnImages();
            this._PlotterOptionsPropertyChangedHook = settings.plotterOptions.hookPropertyChanged(this.optionsPropertyChanged, this);
            this._PlotterOptionsRangeCirclePropertyChangedHook = settings.plotterOptions.hookRangeCirclePropertyChanged(this.optionsRangePropertyChanged, this);
            this._AircraftListUpdatedHook = settings.aircraftList ? settings.aircraftList.hookUpdated(this.refreshMarkersOnListUpdate, this) : null;
            this._AircraftListFetchingListHook = settings.aircraftList ? settings.aircraftList.hookFetchingList(this.fetchingList, this) : null;
            this._SelectedAircraftChangedHook = settings.aircraftList ? settings.aircraftList.hookSelectedAircraftChanged(this.refreshSelectedAircraft, this) : null;
            this._FlightLevelHeightUnitChangedHook = this._UnitDisplayPreferences.hookFlightLevelHeightUnitChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); });
            this._FlightLevelTransitionAltitudeChangedHook = this._UnitDisplayPreferences.hookFlightLevelTransitionAltitudeChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); });
            this._FlightLevelTransitionHeightUnitChangedHook = this._UnitDisplayPreferences.hookFlightLevelTransitionHeightUnitChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); });
            this._HeightUnitChangedHook = this._UnitDisplayPreferences.hookHeightUnitChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.Altitude); });
            this._SpeedUnitChangedHook = this._UnitDisplayPreferences.hookSpeedUnitChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.Speed); });
            this._ShowVsiInSecondsHook = this._UnitDisplayPreferences.hookShowVerticalSpeedPerSecondChanged(function () { return _this.refreshMarkersIfUsingPinText(VRS.RenderProperty.VerticalSpeed); });
            this._MapIdleHook = this._Map.hookIdle(function () { return _this.refreshMarkers(null, null); });
            this._MapMarkerClickedHook = this._Map.hookMarkerClicked(function (event, data) { return _this.selectAircraftById((data.id)); });
            this._CurrentLocationChangedHook = VRS.currentLocation ? VRS.currentLocation.hookCurrentLocationChanged(this.currentLocationChanged, this) : null;
            this._ConfigurationChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.configurationChanged, this);
        }
        AircraftPlotter.prototype.getName = function () {
            return this._Settings.name;
        };
        AircraftPlotter.prototype.getMap = function () {
            return this._Map;
        };
        AircraftPlotter.prototype.getMapMarkerClusterer = function () {
            return this._MapMarkerClusterer;
        };
        AircraftPlotter.prototype.getHideTrailsAtMaxZoom = function () {
            return !!this._MapMarkerClusterer;
        };
        AircraftPlotter.prototype.getHideTrailsMaxZoom = function () {
            return !this._MapMarkerClusterer ? -1 : this._MapMarkerClusterer.getMaxZoom();
        };
        AircraftPlotter.prototype.getMovingMap = function () {
            return this._MovingMap;
        };
        AircraftPlotter.prototype.setMovingMap = function (value) {
            if (value !== this._MovingMap) {
                this._MovingMap = value;
                if (value) {
                    this.moveMapToSelectedAircraft();
                }
            }
        };
        AircraftPlotter.prototype.dispose = function () {
            this.destroyRangeCircles();
            if (this._PlotterOptionsPropertyChangedHook)
                this._Settings.plotterOptions.unhook(this._PlotterOptionsPropertyChangedHook);
            if (this._PlotterOptionsRangeCirclePropertyChangedHook)
                this._Settings.plotterOptions.unhook(this._PlotterOptionsRangeCirclePropertyChangedHook);
            if (this._AircraftListUpdatedHook)
                this._Settings.aircraftList.unhook(this._AircraftListUpdatedHook);
            if (this._AircraftListFetchingListHook)
                this._Settings.aircraftList.unhook(this._AircraftListFetchingListHook);
            if (this._SelectedAircraftChangedHook)
                this._Settings.aircraftList.unhook(this._SelectedAircraftChangedHook);
            if (this._FlightLevelHeightUnitChangedHook)
                this._UnitDisplayPreferences.unhook(this._FlightLevelHeightUnitChangedHook);
            if (this._FlightLevelTransitionAltitudeChangedHook)
                this._UnitDisplayPreferences.unhook(this._FlightLevelTransitionAltitudeChangedHook);
            if (this._FlightLevelTransitionHeightUnitChangedHook)
                this._UnitDisplayPreferences.unhook(this._FlightLevelTransitionHeightUnitChangedHook);
            if (this._HeightUnitChangedHook)
                this._UnitDisplayPreferences.unhook(this._HeightUnitChangedHook);
            if (this._SpeedUnitChangedHook)
                this._UnitDisplayPreferences.unhook(this._SpeedUnitChangedHook);
            if (this._ShowVsiInSecondsHook)
                this._UnitDisplayPreferences.unhook(this._ShowVsiInSecondsHook);
            if (this._MapIdleHook)
                this._Map.unhook(this._MapIdleHook);
            if (this._MapMarkerClickedHook)
                this._Map.unhook(this._MapMarkerClickedHook);
            if (this._CurrentLocationChangedHook)
                VRS.currentLocation.unhook(this._CurrentLocationChangedHook);
            if (this._ConfigurationChangedHook)
                VRS.globalDispatch.unhook(this._ConfigurationChangedHook);
        };
        AircraftPlotter.prototype.configureSuppressTextOnImages = function () {
            var originalValue = this._SuppressTextOnImages;
            this._SuppressTextOnImages = this._Settings.suppressTextOnImages;
            if (VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    if (this._SuppressTextOnImages === undefined) {
                        this._SuppressTextOnImages = config.UseMarkerLabels;
                    }
                }
            }
            if (this._SuppressTextOnImages === undefined) {
                this._SuppressTextOnImages = false;
            }
            return originalValue != this._SuppressTextOnImages;
        };
        AircraftPlotter.prototype.suspend = function (onOff) {
            onOff = !!onOff;
            if (this._Suspended !== onOff) {
                this._Suspended = onOff;
                if (!this._Suspended) {
                    this.refreshMarkers(null, null, true);
                    this.refreshRangeCircles(true);
                }
            }
        };
        AircraftPlotter.prototype.plot = function (refreshAllMarkers, ignoreBounds) {
            this.refreshMarkers(null, null, !!refreshAllMarkers, !!ignoreBounds);
        };
        ;
        AircraftPlotter.prototype.getPlottedAircraftIds = function () {
            var result = [];
            for (var aircraftId in this._PlottedDetail) {
                result.push(Number(aircraftId));
            }
            return result;
        };
        AircraftPlotter.prototype.refreshMarkers = function (newAircraft, oldAircraft, alwaysRefreshIcon, ignoreBounds) {
            var _this = this;
            var unusedAircraft = null;
            if (oldAircraft) {
                this.removeOldMarkers(oldAircraft);
            }
            else {
                unusedAircraft = new VRS.AircraftCollection();
                for (var aircraftId in this._PlottedDetail) {
                    unusedAircraft[aircraftId] = this._PlottedDetail[aircraftId].aircraft;
                }
            }
            var bounds = this._Map.getBounds();
            if (bounds || ignoreBounds) {
                var mapZoomLevel = this._Map.getZoom();
                var selectedAircraft = this._GetSelectedAircraft();
                this._GetAircraft().foreachAircraft(function (aircraft) {
                    _this.refreshAircraftMarker(aircraft, alwaysRefreshIcon, ignoreBounds, bounds, mapZoomLevel, selectedAircraft && selectedAircraft === aircraft);
                    if (unusedAircraft && unusedAircraft[aircraft.id]) {
                        unusedAircraft[aircraft.id] = undefined;
                    }
                });
                this.moveMapToSelectedAircraft(selectedAircraft);
            }
            if (unusedAircraft) {
                this.removeOldMarkers(unusedAircraft);
            }
            if (this._MapMarkerClusterer) {
                this._MapMarkerClusterer.repaint();
            }
        };
        AircraftPlotter.prototype.refreshAircraftMarker = function (aircraft, forceRefresh, ignoreBounds, bounds, mapZoomLevel, isSelectedAircraft) {
            if ((ignoreBounds || bounds) && aircraft.hasPosition()) {
                var isStale = aircraft.positionStale.val;
                var position = isStale ? null : aircraft.getPosition();
                var isInBounds = isStale ? false : ignoreBounds || aircraft.positionWithinBounds(bounds);
                var plotAircraft = !isStale && (isInBounds || (isSelectedAircraft && VRS.globalOptions.aircraftMarkerAlwaysPlotSelected));
                if (plotAircraft && !aircraft.isAircraftSpecies() && mapZoomLevel < this._Settings.hideNonAircraftZoomLevel)
                    plotAircraft = false;
                var details = this._PlottedDetail[aircraft.id];
                if (details && details.aircraft !== aircraft) {
                    this.removeDetails(details);
                    details = undefined;
                }
                if (details) {
                    if (!plotAircraft) {
                        this.removeDetails(details);
                    }
                    else {
                        var marker = details.mapMarker;
                        marker.setPosition(position);
                        if (forceRefresh || this.haveIconDetailsChanged(details, mapZoomLevel)) {
                            var icon = this.createIcon(details, mapZoomLevel, isSelectedAircraft);
                            if (icon) {
                                marker.setIcon(icon);
                                var zIndex = isSelectedAircraft ? 101 : 100;
                                if (zIndex !== marker.getZIndex()) {
                                    marker.setZIndex(zIndex);
                                }
                                if (marker.isMarkerWithLabel) {
                                    if (icon.labelAnchor && (!details.mapIcon || (details.mapIcon.labelAnchor.x !== icon.labelAnchor.x || details.mapIcon.labelAnchor.y !== icon.labelAnchor.y))) {
                                        marker.setLabelAnchor(icon.labelAnchor);
                                    }
                                }
                                details.mapIcon = icon;
                            }
                        }
                        if (forceRefresh || this.haveTooltipDetailsChanged(details)) {
                            marker.setTooltip(this.getTooltip(details));
                        }
                        if (forceRefresh || this.haveLabelDetailsChanged(details)) {
                            this.createLabel(details);
                        }
                        this.updateTrail(details, isSelectedAircraft, mapZoomLevel, forceRefresh);
                    }
                }
                else if (plotAircraft) {
                    details = new PlottedDetail(aircraft);
                    details.mapIcon = this.createIcon(details, mapZoomLevel, isSelectedAircraft);
                    var markerOptions = {
                        clickable: true,
                        draggable: false,
                        flat: true,
                        icon: details.mapIcon,
                        visible: true,
                        position: position,
                        tooltip: this.getTooltip(details),
                        zIndex: isSelectedAircraft ? 101 : 100
                    };
                    if (this._SuppressTextOnImages) {
                        markerOptions.useMarkerWithLabel = true;
                        markerOptions.mwlLabelInBackground = true;
                        markerOptions.mwlLabelClass = 'markerLabel';
                    }
                    details.mapMarker = this._Map.addMarker(aircraft.id, markerOptions);
                    this.createLabel(details);
                    this.updateTrail(details, isSelectedAircraft, mapZoomLevel, forceRefresh);
                    this._PlottedDetail[aircraft.id] = details;
                    if (this._MapMarkerClusterer) {
                        this._MapMarkerClusterer.addMarker(details.mapMarker, true);
                    }
                }
            }
        };
        AircraftPlotter.prototype.removeOldMarkers = function (oldAircraft) {
            var _this = this;
            oldAircraft.foreachAircraft(function (aircraft) {
                var details = _this._PlottedDetail[aircraft.id];
                if (details) {
                    _this.removeDetails(details);
                }
            });
        };
        AircraftPlotter.prototype.removeAllMarkers = function () {
            var allPlottedAircraftIds = this.getPlottedAircraftIds();
            var length = allPlottedAircraftIds.length;
            for (var i = 0; i < length; ++i) {
                var aircraftId = allPlottedAircraftIds[i];
                var details = this._PlottedDetail[aircraftId];
                this.removeDetails(details);
            }
        };
        AircraftPlotter.prototype.isAircraftBeingPlotted = function (aircraft) {
            return !!(aircraft && this._PlottedDetail[aircraft.id]);
        };
        AircraftPlotter.prototype.removeDetails = function (details) {
            if (details.mapMarker) {
                if (this._MapMarkerClusterer) {
                    this._MapMarkerClusterer.removeMarker(details.mapMarker, true);
                }
                this._Map.destroyMarker(details.mapMarker);
            }
            this.removeTrail(details);
            details.mapIcon = null;
            details.aircraft = null;
            details.mapMarker = null;
            details.mapPolylines = [];
            delete this._PlottedDetail[details.id];
            details.id = null;
            details.pinTexts = null;
            details.iconUrl = null;
        };
        AircraftPlotter.prototype.haveIconDetailsChanged = function (details, mapZoomLevel) {
            var result = false;
            var aircraft = details.aircraft;
            if (!result) {
                if (this.allowIconRotation()) {
                    result = details.iconRotation === undefined || details.iconRotation !== this.getIconHeading(aircraft);
                }
                else {
                    if (details.iconRotation !== undefined) {
                        result = true;
                    }
                }
            }
            if (!result) {
                if (this.allowIconAltitudeStalk(mapZoomLevel)) {
                    result = details.iconAltitudeStalkHeight == undefined || details.iconAltitudeStalkHeight !== this.getIconAltitudeStalkHeight(aircraft);
                }
                else {
                    if (details.iconAltitudeStalkHeight !== undefined) {
                        result = true;
                    }
                }
            }
            if (!result && !this._SuppressTextOnImages && (!details.mapMarker || !details.mapMarker.isMarkerWithLabel)) {
                if (this.allowPinTexts()) {
                    result = this.havePinTextDependenciesChanged(aircraft);
                }
                else {
                    if (details.pinTexts.length !== 0) {
                        result = true;
                    }
                }
            }
            return result;
        };
        AircraftPlotter.prototype.createIcon = function (details, mapZoomLevel, isSelectedAircraft) {
            var aircraft = details.aircraft;
            var marker = this.getAircraftMarkerDetails(aircraft);
            var useSvg = marker.useEmbeddedSvg();
            var size = marker.getSize();
            size = { width: size.width, height: size.height };
            var anchorY = Math.floor(size.height / 2);
            var suppressPinText = this._SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel);
            if (!this.allowIconRotation() || !marker.getCanRotate()) {
                details.iconRotation = undefined;
            }
            else {
                details.iconRotation = this.getIconHeading(aircraft);
            }
            var blankPixelsAtBottom = 0;
            if (!suppressPinText) {
                if (!this.allowPinTexts()) {
                    if (details.pinTexts.length > 0) {
                        details.pinTexts = [];
                    }
                }
                else {
                    details.pinTexts = this.getPinTexts(aircraft);
                    size.height += (this._Settings.pinTextLineHeight * details.pinTexts.length);
                    if (size.width <= this._Settings.pinTextMarkerWidth) {
                        size.width = this._Settings.pinTextMarkerWidth;
                    }
                    else {
                        size.width += size.width % 4;
                    }
                    blankPixelsAtBottom = size.height % 4;
                    size.height += blankPixelsAtBottom;
                }
            }
            var pinTextLines = suppressPinText ? 0 : details.pinTexts.length;
            var hasAltitudeStalk = false;
            if (!marker.getIsAircraft() || !this.allowIconAltitudeStalk(mapZoomLevel)) {
                details.iconAltitudeStalkHeight = undefined;
            }
            else {
                hasAltitudeStalk = true;
                details.iconAltitudeStalkHeight = this.getIconAltitudeStalkHeight(aircraft);
                size.height += details.iconAltitudeStalkHeight + 2;
                anchorY = size.height - blankPixelsAtBottom;
            }
            var centreX = Math.floor(size.width / 2);
            var labelAnchor = null;
            if (suppressPinText) {
                labelAnchor = {
                    x: size.width - centreX,
                    y: hasAltitudeStalk ? 3 : anchorY - size.height
                };
            }
            var requestSize = size;
            var multiplier = 1;
            if (!useSvg && VRS.browserHelper.isHighDpi()) {
                multiplier = 2;
                requestSize = {
                    width: size.width * multiplier,
                    height: size.height * multiplier
                };
            }
            var url = marker.getFolder();
            url += '/top';
            url += '/Wdth-' + requestSize.width;
            url += '/Hght-' + requestSize.height;
            if (VRS.browserHelper.isHighDpi()) {
                url += '/hiDpi';
            }
            if (details.iconRotation || details.iconRotation === 0) {
                url += '/Rotate-' + details.iconRotation;
            }
            if (hasAltitudeStalk) {
                url += '/Alt-' + (details.iconAltitudeStalkHeight * multiplier);
                url += '/CenX-' + (centreX * multiplier);
            }
            if (pinTextLines > 0) {
                for (var i = 0; i < pinTextLines; ++i) {
                    url += '/PL' + (i + 1) + '-' + encodeURIComponent(details.pinTexts[i]);
                }
            }
            url += '/' + (isSelectedAircraft ? marker.getSelectedFileName() : marker.getNormalFileName());
            var urlChanged = details.iconUrl !== url;
            details.iconUrl = url;
            if (useSvg && urlChanged) {
                var svg = this._SvgGenerator.generateAircraftMarker(marker.getEmbeddedSvg(), marker.getSvgFillColour(aircraft, isSelectedAircraft), requestSize.width, requestSize.height, details.iconRotation, hasAltitudeStalk, pinTextLines > 0 ? details.pinTexts : null, this._Settings.pinTextLineHeight);
                var svgText = svg.outerHTML;
                url = 'data:image/svg+xml;charset=UTF-8;base64,' + btoa(svgText);
                svg.innerHTML = '';
                svg = null;
            }
            return !urlChanged ? null : new VRS.MapIcon(url, size, { x: centreX, y: anchorY }, { x: 0, y: 0 }, size, labelAnchor);
        };
        AircraftPlotter.prototype.getAircraftMarkerDetails = function (aircraft) {
            var result = null;
            if (aircraft) {
                var markers = this._Settings.aircraftMarkers;
                var onlyUsePre22Icons = this._Settings.plotterOptions.getOnlyUsePre22Icons();
                var length = markers.length;
                for (var i = 0; i < length; ++i) {
                    result = markers[i];
                    if (onlyUsePre22Icons && !result.getIsPre22Icon()) {
                        continue;
                    }
                    if (result.matchesAircraft(aircraft)) {
                        break;
                    }
                }
            }
            return result;
        };
        AircraftPlotter.prototype.allowIconRotation = function () {
            return this._Settings.allowRotation;
        };
        AircraftPlotter.prototype.getIconHeading = function (aircraft) {
            var rotationGranularity = this._Settings.rotationGranularity;
            var heading = aircraft.heading.val;
            if (isNaN(heading)) {
                heading = 0;
            }
            else {
                heading = Math.round(heading / rotationGranularity) * rotationGranularity;
            }
            return heading;
        };
        AircraftPlotter.prototype.allowIconAltitudeStalk = function (mapZoomLevel) {
            return VRS.globalOptions.aircraftMarkerAllowAltitudeStalk &&
                this._Settings.plotterOptions.getShowAltitudeStalk() &&
                (!this._Settings.plotterOptions.getSuppressAltitudeStalkWhenZoomedOut() || mapZoomLevel >= this._Settings.suppressAltitudeStalkAboveZoom);
        };
        AircraftPlotter.prototype.getIconAltitudeStalkHeight = function (aircraft) {
            var result = aircraft.altitude.val;
            if (isNaN(result)) {
                result = 0;
            }
            else {
                result = Math.max(0, Math.min(result, 35000));
                result = Math.round(result / 2500) * 5;
            }
            return result;
        };
        AircraftPlotter.prototype.allowPinTexts = function () {
            var result = this._Settings.plotterOptions.getShowPinText();
            if (result && VRS.serverConfig) {
                result = VRS.serverConfig.pinTextEnabled();
            }
            return result;
        };
        AircraftPlotter.prototype.havePinTextDependenciesChanged = function (aircraft) {
            var result = false;
            var pinTexts = this._Settings.plotterOptions.getPinTexts();
            var length = pinTexts.length;
            for (var i = 0; i < length; ++i) {
                var handler = VRS.renderPropertyHandlers[pinTexts[i]];
                if (handler && handler.hasChangedCallback(aircraft)) {
                    result = true;
                    break;
                }
            }
            return result;
        };
        AircraftPlotter.prototype.getPinTexts = function (aircraft) {
            var result = [];
            var suppressBlankLines = this._Settings.plotterOptions.getHideEmptyPinTextLines();
            if (this._Settings.getCustomPinTexts) {
                result = this._Settings.getCustomPinTexts(aircraft) || [];
            }
            else {
                var options = {
                    unitDisplayPreferences: this._UnitDisplayPreferences,
                    distinguishOnGround: true
                };
                var length = Math.min(this._Settings.plotterOptions.getPinTextLines(), VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
                for (var i = 0; i < length; ++i) {
                    var renderProperty = this._Settings.plotterOptions.getPinText(i);
                    if (renderProperty === VRS.RenderProperty.None)
                        continue;
                    var handler = VRS.renderPropertyHandlers[renderProperty];
                    var text = handler ? handler.contentCallback(aircraft, options, VRS.RenderSurface.Marker) || '' : '';
                    if (!suppressBlankLines || text) {
                        result.push(text);
                    }
                }
            }
            return result;
        };
        AircraftPlotter.prototype.haveLabelDetailsChanged = function (details) {
            var result = false;
            if (this._SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel)) {
                if (this.allowPinTexts()) {
                    result = this.havePinTextDependenciesChanged(details.aircraft);
                }
                else {
                    if (details.pinTexts.length !== 0) {
                        result = true;
                    }
                }
            }
            return result;
        };
        AircraftPlotter.prototype.createLabel = function (details) {
            if (this._SuppressTextOnImages && details.mapMarker && details.mapMarker.isMarkerWithLabel) {
                if (this.allowPinTexts()) {
                    details.pinTexts = this.getPinTexts(details.aircraft);
                }
                else {
                    if (details.pinTexts.length > 0) {
                        details.pinTexts = [];
                    }
                }
                var labelText = '';
                var length = details.pinTexts.length;
                for (var i = 0; i < length; ++i) {
                    if (labelText.length) {
                        labelText += '<br/>';
                    }
                    labelText += '<span>&nbsp;' + VRS.stringUtility.htmlEscape(details.pinTexts[i]) + '&nbsp;</span>';
                }
                var marker = details.mapMarker;
                if (labelText.length === 0 || !details.mapIcon) {
                    marker.setLabelVisible(false);
                }
                else {
                    if (!marker.getLabelVisible()) {
                        marker.setLabelVisible(true);
                    }
                    marker.setLabelAnchor(details.mapIcon.labelAnchor);
                    marker.setLabelContent(labelText);
                }
            }
        };
        AircraftPlotter.prototype.updateTrail = function (details, isAircraftSelected, mapZoomLevel, forceRefresh) {
            if (VRS.globalOptions.suppressTrails) {
                return;
            }
            var aircraft = details.aircraft;
            var showTrails = false;
            switch (this._Settings.plotterOptions.getTrailDisplay()) {
                case VRS.TrailDisplay.None: break;
                case VRS.TrailDisplay.AllAircraft:
                    showTrails = true;
                    break;
                case VRS.TrailDisplay.SelectedOnly:
                    showTrails = isAircraftSelected;
                    break;
            }
            if (showTrails && !aircraft.isAircraftSpecies() && !VRS.globalOptions.aircraftMarkerShowNonAircraftTrails) {
                showTrails = false;
            }
            if (showTrails && mapZoomLevel <= this.getHideTrailsMaxZoom()) {
                showTrails = false;
            }
            if (forceRefresh) {
                this.removeTrail(details);
            }
            if (!showTrails) {
                if (details.mapPolylines.length) {
                    this.removeTrail(details);
                }
            }
            else {
                var trailType = this._Settings.plotterOptions.getTrailType();
                var trail;
                var isMonochrome = false;
                var isFullTrail = false;
                switch (trailType) {
                    case VRS.TrailType.Full:
                        isMonochrome = true;
                    case VRS.TrailType.FullAltitude:
                    case VRS.TrailType.FullSpeed:
                        isFullTrail = true;
                        trail = aircraft.fullTrail;
                        break;
                    case VRS.TrailType.Short:
                        isMonochrome = true;
                    default:
                        trail = aircraft.shortTrail;
                        break;
                }
                if (details.mapPolylines.length && details.polylineTrailType !== trailType) {
                    this.removeTrail(details);
                }
                if (trail.trimStartCount) {
                    this.trimShortTrailPoints(details, trail);
                }
                var polylines = details.mapPolylines;
                var lastLine = polylines.length ? polylines[polylines.length - 1] : null;
                if (!lastLine) {
                    this.createTrail(details, trail, trailType, isAircraftSelected, isMonochrome);
                }
                else {
                    var width = this.getTrailWidth(isAircraftSelected);
                    if (lastLine.getStrokeWeight() !== width) {
                        var length = polylines.length;
                        for (var i = 0; i < length; ++i) {
                            polylines[i].setStrokeWeight(width);
                        }
                    }
                    if (isMonochrome) {
                        var colour = this.getMonochromeTrailColour(isAircraftSelected);
                        if (lastLine.getStrokeColour() !== colour) {
                            lastLine.setStrokeColour(colour);
                        }
                    }
                    if (details.polylinePathUpdateCounter !== aircraft.updateCounter && trail.chg) {
                        this.synchroniseAircraftAndMapPolylinePaths(details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail);
                    }
                }
                details.polylineTrailType = trailType;
                details.polylinePathUpdateCounter = aircraft.updateCounter;
            }
        };
        AircraftPlotter.prototype.getMonochromeTrailColour = function (isAircraftSelected) {
            return isAircraftSelected ? this._Settings.selectedTrailColour : this._Settings.normalTrailColour;
        };
        AircraftPlotter.prototype.getCoordinateTrailColour = function (coordinate, trailType) {
            var result = null;
            switch (trailType) {
                case VRS.TrailType.FullAltitude:
                case VRS.TrailType.ShortAltitude:
                    result = VRS.colourHelper.colourToCssString(VRS.colourHelper.getColourWheelScale(coordinate.altitude, VRS.globalOptions.aircraftMarkerAltitudeTrailLow, VRS.globalOptions.aircraftMarkerAltitudeTrailHigh, true, true));
                    break;
                case VRS.TrailType.FullSpeed:
                case VRS.TrailType.ShortSpeed:
                    result = VRS.colourHelper.colourToCssString(VRS.colourHelper.getColourWheelScale(coordinate.speed, VRS.globalOptions.aircraftMarkerSpeedTrailLow, VRS.globalOptions.aircraftMarkerSpeedTrailHigh, true, true));
                    break;
                default:
                    throw 'The trail type ' + trailType + ' does not show multiple colours on a single trail';
            }
            return result;
        };
        AircraftPlotter.prototype.getTrailWidth = function (isAircraftSelected) {
            return isAircraftSelected ? this._Settings.selectedTrailWidth : this._Settings.normalTrailWidth;
        };
        AircraftPlotter.prototype.getTrailPath = function (trail, start, count, aircraft, trailType, isAircraftSelected, isMonochrome) {
            var result = [];
            var length = trail.arr.length;
            if (start === undefined)
                start = 0;
            if (start > length)
                throw 'Cannot get the trail from index ' + start + ', there are only ' + length + ' coordinates';
            if (count === undefined)
                count = length - start;
            if (start + count > length)
                throw 'Cannot get ' + count + ' points from index ' + start + ', there are only ' + length + ' coordinates';
            var colour = aircraft && isMonochrome ? this.getMonochromeTrailColour(isAircraftSelected) : null;
            var end = start + count;
            for (var i = start; i < end; ++i) {
                var coord = trail.arr[i];
                if (aircraft && !isMonochrome) {
                    colour = this.getCoordinateTrailColour(coord, trailType);
                }
                result.push({ lat: coord.lat, lng: coord.lng, colour: colour });
            }
            return result;
        };
        AircraftPlotter.prototype.createTrail = function (details, trail, trailType, isAircraftSelected, isMonochrome) {
            if (details.mapPolylines.length)
                throw 'Cannot create a trail for aircraft ID ' + details.id + ', one already exists';
            var aircraft = details.aircraft;
            var path = this.getTrailPath(trail, undefined, undefined, aircraft, trailType, isAircraftSelected, isMonochrome);
            if (path.length) {
                var weight = this.getTrailWidth(isAircraftSelected);
                if (!isMonochrome) {
                    this.addMultiColouredPolylines(details, path, weight, null);
                }
                else {
                    details.mapPolylines.push(this._Map.addPolyline(aircraft.id, {
                        clickable: false,
                        draggable: false,
                        editable: false,
                        geodesic: true,
                        strokeColour: path[0].colour,
                        strokeWeight: weight,
                        strokeOpacity: 1,
                        path: path
                    }));
                }
            }
        };
        AircraftPlotter.prototype.addMultiColouredPolylines = function (details, path, weight, fromCoord) {
            var aircraft = details.aircraft;
            var segments = [];
            if (fromCoord) {
                segments.push({
                    lat: fromCoord.lat,
                    lng: fromCoord.lng
                });
            }
            var length = path.length;
            var nextSegment = null;
            var firstAdd = true;
            for (var i = 0; i < length; ++i) {
                var segment = nextSegment === null ? path[i] : nextSegment;
                nextSegment = i + 1 === length ? null : path[i + 1];
                segments.push(segment);
                if (nextSegment && nextSegment.colour === segment.colour) {
                    continue;
                }
                if (nextSegment) {
                    segments.push(nextSegment);
                }
                if (segments.length > 1) {
                    if (firstAdd) {
                        firstAdd = false;
                        var lastLine = details.mapPolylines.length ? details.mapPolylines[details.mapPolylines.length - 1] : null;
                        if (lastLine) {
                            var firstPoint = lastLine.getFirstLatLng();
                            var firstSegment = segments[0];
                            if (firstPoint && Math.abs(firstPoint.lat - firstSegment.lat) < 0.0000001 && Math.abs(firstPoint.lng - firstSegment.lng) < 0.0000001) {
                                this._Map.destroyPolyline(lastLine);
                                details.mapPolylines.splice(-1, 1);
                            }
                        }
                    }
                    var id = aircraft.id.toString() + '$' + details.nextPolylineId++;
                    details.mapPolylines.push(this._Map.addPolyline(id, {
                        clickable: false,
                        draggable: false,
                        editable: false,
                        geodesic: true,
                        strokeColour: segment.colour,
                        strokeWeight: weight,
                        strokeOpacity: 1,
                        path: segments
                    }));
                }
                segments = [];
            }
        };
        AircraftPlotter.prototype.removeTrail = function (details) {
            var length = details.mapPolylines.length;
            if (length) {
                for (var i = 0; i < length; ++i) {
                    this._Map.destroyPolyline(details.mapPolylines[i]);
                }
                details.mapPolylines = [];
                details.polylinePathUpdateCounter = undefined;
                details.polylineTrailType = undefined;
            }
        };
        AircraftPlotter.prototype.synchroniseAircraftAndMapPolylinePaths = function (details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail) {
            var polylines = details.mapPolylines;
            var polylinesLength = polylines.length;
            var trailLength = trail.arr.length;
            if (isFullTrail && trail.chg && trail.chgIdx === -1 && trailLength > 0 && trail.arr[trailLength - 1].chg) {
                var changedTrail = trail.arr[trailLength - 1];
                this._Map.replacePolylinePointAt(polylines[polylinesLength - 1], -1, { lat: changedTrail.lat, lng: changedTrail.lng });
            }
            else {
                if (trail.chgIdx !== -1 && trail.chgIdx < trail.arr.length) {
                    var path = this.getTrailPath(trail, trail.chgIdx, undefined, details.aircraft, trailType, isAircraftSelected, isMonochrome);
                    if (isMonochrome) {
                        this._Map.appendToPolyline(polylines[polylinesLength - 1], path, false);
                    }
                    else {
                        var weight = this.getTrailWidth(isAircraftSelected);
                        var fromCoord = trail.chgIdx > 0 ? trail.arr[trail.chgIdx - 1] : null;
                        this.addMultiColouredPolylines(details, path, weight, fromCoord);
                    }
                }
            }
        };
        AircraftPlotter.prototype.trimShortTrailPoints = function (details, trail) {
            var countRemove = trail.trimStartCount;
            var polylines = details.mapPolylines;
            var countLines = polylines.length;
            while (countRemove > 0 && countLines) {
                var oldestLine = polylines[0];
                var removeState = this._Map.trimPolyline(oldestLine, countRemove, true);
                countRemove -= removeState.countRemoved;
                if (removeState.emptied || !removeState.countRemoved) {
                    polylines.splice(0, 1);
                    --countLines;
                    this._Map.destroyPolyline(oldestLine);
                }
            }
        };
        AircraftPlotter.prototype.haveTooltipDetailsChanged = function (details) {
            var result = false;
            if (this._Settings.showTooltips) {
                var aircraft = details.aircraft;
                result = aircraft.icao.chg ||
                    aircraft.modelIcao.chg ||
                    aircraft.registration.chg ||
                    aircraft.callsign.chg ||
                    aircraft.callsignSuspect.chg ||
                    aircraft.operator.chg ||
                    aircraft.operatorIcao.chg ||
                    aircraft.from.chg ||
                    aircraft.to.chg ||
                    aircraft.via.chg;
            }
            return result;
        };
        AircraftPlotter.prototype.getTooltip = function (details) {
            var result = '';
            if (this._Settings.showTooltips) {
                var aircraft = details.aircraft;
                var addToResult = function (text) { if (text) {
                    if (result)
                        result += ' || ';
                    result += text;
                } };
                addToResult(aircraft.formatIcao());
                addToResult(aircraft.formatModelIcao());
                addToResult(aircraft.formatRegistration());
                addToResult(aircraft.formatCallsign(VRS.globalOptions.aircraftFlagUncertainCallsigns));
                addToResult(aircraft.formatOperatorIcaoAndName());
                addToResult(aircraft.formatRouteFull());
            }
            return result;
        };
        AircraftPlotter.prototype.selectAircraftById = function (id) {
            if (this._Settings.aircraftList) {
                var details = this._PlottedDetail[id];
                if (details) {
                    this._Settings.aircraftList.setSelectedAircraft(details.aircraft, true);
                }
            }
        };
        AircraftPlotter.prototype.moveMapToSelectedAircraft = function (selectedAircraft) {
            if (this._MovingMap) {
                if (!selectedAircraft) {
                    selectedAircraft = this._GetSelectedAircraft();
                }
                this.moveMapToAircraft(selectedAircraft);
            }
        };
        AircraftPlotter.prototype.moveMapToAircraft = function (aircraft) {
            if (aircraft && aircraft.hasPosition()) {
                this._Map.setCenter(aircraft.getPosition());
            }
        };
        AircraftPlotter.prototype.refreshRangeCircles = function (forceRefresh) {
            var currentLocation = VRS.currentLocation ? VRS.currentLocation.getCurrentLocation() : null;
            if (!currentLocation || !this._Settings.allowRangeCircles || !this._Settings.plotterOptions.getShowRangeCircles()) {
                this.destroyRangeCircles();
            }
            else {
                if (forceRefresh || !this._RangeCircleCentre || this._RangeCircleCentre.lat !== currentLocation.lat || this._RangeCircleCentre.lng !== currentLocation.lng) {
                    var plotterOptions = this._Settings.plotterOptions;
                    var baseCircleId = -1000;
                    var intervalMetres = VRS.unitConverter.convertDistance(plotterOptions.getRangeCircleInterval(), plotterOptions.getRangeCircleDistanceUnit(), VRS.Distance.Kilometre) * 1000;
                    var countCircles = plotterOptions.getRangeCircleCount();
                    for (var i = 0; i < countCircles; ++i) {
                        var isOdd = i % 2 === 0;
                        var circle = this._RangeCircleCircles.length > i ? this._RangeCircleCircles[i] : null;
                        var radius = intervalMetres * (i + 1);
                        var colour = isOdd ? plotterOptions.getRangeCircleOddColour() : plotterOptions.getRangeCircleEvenColour();
                        var weight = isOdd ? plotterOptions.getRangeCircleOddWeight() : plotterOptions.getRangeCircleEvenWeight();
                        if (circle) {
                            circle.setCenter(currentLocation);
                            circle.setRadius(radius);
                            circle.setStrokeColor(colour);
                            circle.setStrokeWeight(weight);
                        }
                        else {
                            this._RangeCircleCircles.push(this._Map.addCircle(baseCircleId - i, {
                                center: currentLocation,
                                radius: radius,
                                strokeColor: colour,
                                strokeWeight: weight
                            }));
                        }
                    }
                    if (countCircles < this._RangeCircleCircles.length) {
                        for (var i = countCircles; i < this._RangeCircleCircles.length; ++i) {
                            this._Map.destroyCircle(this._RangeCircleCircles[i]);
                        }
                        this._RangeCircleCircles.splice(countCircles, this._RangeCircleCircles.length - countCircles);
                    }
                    this._RangeCircleCentre = currentLocation;
                }
            }
        };
        AircraftPlotter.prototype.destroyRangeCircles = function () {
            var _this = this;
            if (this._RangeCircleCircles.length) {
                $.each(this._RangeCircleCircles, function (idx, circle) {
                    _this._Map.destroyCircle(circle);
                });
                this._RangeCircleCircles = [];
                this._RangeCircleCentre = null;
            }
        };
        AircraftPlotter.prototype.getAircraftMarker = function (aircraft) {
            var result = null;
            if (aircraft) {
                var detail = this._PlottedDetail[aircraft.id];
                if (detail) {
                    result = detail.mapMarker;
                }
            }
            return result;
        };
        AircraftPlotter.prototype.getAircraftForMarkerId = function (mapMarkerId) {
            var result = null;
            var details = this._PlottedDetail[mapMarkerId];
            if (details) {
                result = details.aircraft;
            }
            return result;
        };
        AircraftPlotter.prototype.diagnosticsGetPlottedDetail = function (aircraft) {
            return this._PlottedDetail[aircraft.id];
        };
        AircraftPlotter.prototype.optionsPropertyChanged = function () {
            if (this._MapMarkerClusterer) {
                var newMaxZoom = this._Settings.plotterOptions.getAircraftMarkerClustererMaxZoom();
                if (newMaxZoom !== this._MapMarkerClusterer.getMaxZoom()) {
                    this._MapMarkerClusterer.setMaxZoom(newMaxZoom);
                }
            }
            if (!this._Suspended) {
                this.refreshMarkers(null, null, true);
            }
        };
        AircraftPlotter.prototype.optionsRangePropertyChanged = function () {
            if (!this._Suspended) {
                this.refreshRangeCircles(true);
            }
        };
        AircraftPlotter.prototype.fetchingList = function (xhrParams) {
            if (!VRS.globalOptions.suppressTrails) {
                var trailType = this._Settings.plotterOptions.getTrailType();
                switch (trailType) {
                    case VRS.TrailType.Full:
                        xhrParams.trFmt = 'f';
                        break;
                    case VRS.TrailType.Short:
                        xhrParams.trFmt = 's';
                        break;
                    case VRS.TrailType.FullAltitude:
                        xhrParams.trFmt = 'fa';
                        break;
                    case VRS.TrailType.ShortAltitude:
                        xhrParams.trFmt = 'sa';
                        break;
                    case VRS.TrailType.FullSpeed:
                        xhrParams.trFmt = 'fs';
                        break;
                    case VRS.TrailType.ShortSpeed:
                        xhrParams.trFmt = 'ss';
                        break;
                }
                if (this._PreviousTrailTypeRequested && this._PreviousTrailTypeRequested !== trailType) {
                    xhrParams.refreshTrails = '1';
                }
                this._PreviousTrailTypeRequested = trailType;
            }
        };
        AircraftPlotter.prototype.refreshMarkersOnListUpdate = function (newAircraft, oldAircraft) {
            if (!this._Suspended) {
                this.refreshMarkers(newAircraft, oldAircraft);
            }
        };
        AircraftPlotter.prototype.refreshSelectedAircraft = function (oldSelectedAircraft) {
            if (!this._Suspended) {
                var bounds = this._Map.getBounds();
                var mapZoomLevel = this._Map.getZoom();
                if (this.isAircraftBeingPlotted(oldSelectedAircraft)) {
                    this.refreshAircraftMarker(oldSelectedAircraft, true, false, bounds, mapZoomLevel, false);
                }
                var selectedAircraft = this._GetSelectedAircraft();
                if (this.isAircraftBeingPlotted(selectedAircraft)) {
                    this.refreshAircraftMarker(selectedAircraft, true, false, bounds, mapZoomLevel, true);
                }
            }
        };
        AircraftPlotter.prototype.refreshMarkersIfUsingPinText = function (renderProperty) {
            if (!this._Suspended) {
                if ($.inArray(renderProperty, this._Settings.plotterOptions.getPinTexts()) !== -1) {
                    this.refreshMarkers(null, null, true);
                }
            }
        };
        AircraftPlotter.prototype.currentLocationChanged = function () {
            if (!this._Suspended) {
                this.refreshRangeCircles();
            }
        };
        AircraftPlotter.prototype.configurationChanged = function () {
            var destroyAndRepaintMarkers = this.configureSuppressTextOnImages();
            if (!this._Suspended) {
                if (destroyAndRepaintMarkers) {
                    this.removeAllMarkers();
                    this.refreshMarkers(null, null, true);
                }
            }
        };
        return AircraftPlotter;
    }());
    VRS.AircraftPlotter = AircraftPlotter;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftPlotter.js.map