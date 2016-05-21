var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.audioEnabled = VRS.globalOptions.audioEnabled !== undefined ? VRS.globalOptions.audioEnabled : true;
    VRS.globalOptions.audioAnnounceSelected = VRS.globalOptions.audioAnnounceSelected !== undefined ? VRS.globalOptions.audioAnnounceSelected : false;
    VRS.globalOptions.audioAnnounceOnlyAutoSelected = VRS.globalOptions.audioAnnounceOnlyAutoSelected !== undefined ? VRS.globalOptions.audioAnnounceOnlyAutoSelected : false;
    VRS.globalOptions.audioDefaultVolume = VRS.globalOptions.audioDefaultVolume !== undefined ? VRS.globalOptions.audioDefaultVolume : 1.0;
    VRS.globalOptions.audioTimeout = VRS.globalOptions.audioTimeout || 60000;
    var AudioWrapper = (function () {
        function AudioWrapper(name) {
            var _this = this;
            this._PausePhrase = ' ';
            this._AutoplayQueue = [];
            this._Playing = null;
            this._PlayingTimeout = 0;
            this._AnnounceSelectedAircraftList = null;
            this._SelectedAircraftChangedHookResult = null;
            this._ListUpdatedHookResult = null;
            this._AnnouncedAircraft = null;
            this._PreviouslyAnnouncedAircraftIds = [];
            this._AnnounceSelected = VRS.globalOptions.audioAnnounceSelected;
            this._AnnounceOnlyAutoSelected = VRS.globalOptions.audioAnnounceOnlyAutoSelected;
            this._Volume = VRS.globalOptions.audioDefaultVolume;
            this._Muted = false;
            this.getName = function () {
                return _this._Name;
            };
            this.getAnnounceSelected = function () {
                return _this._AnnounceSelected;
            };
            this.setAnnounceSelected = function (value) {
                if (_this._AnnounceSelected !== value) {
                    _this._AnnounceSelected = value;
                    if (!_this._AnnounceSelected) {
                        _this._AutoplayQueue = [];
                        _this.stopPlaying();
                    }
                }
            };
            this.getAnnounceOnlyAutoSelected = function () {
                return _this._AnnounceOnlyAutoSelected;
            };
            this.setAnnounceOnlyAutoSelected = function (value) {
                if (_this._AnnounceOnlyAutoSelected !== value) {
                    _this._AnnounceOnlyAutoSelected = value;
                    if (_this._AnnounceSelected && _this._AnnounceOnlyAutoSelected && _this._AnnouncedAircraftIsUserSelected) {
                        _this._AutoplayQueue = [];
                        _this.stopPlaying();
                    }
                }
            };
            this.getVolume = function () {
                return _this._Volume;
            };
            this.setVolume = function (value) {
                if (_this._Volume !== value) {
                    _this._Volume = value;
                    if (_this._Playing) {
                        (_this._Playing[0]).volume = _this._Volume;
                    }
                }
            };
            this.getMuted = function () {
                return _this._Muted;
            };
            this.setMuted = function (value) {
                if (_this._Muted !== value) {
                    _this._Muted = value;
                    if (_this._Playing) {
                        (_this._Playing[0]).muted = _this._Muted;
                    }
                }
            };
            this.dispose = function () {
                if (_this._SelectedAircraftChangedHookResult) {
                    _this._AnnounceSelectedAircraftList.unhook(_this._SelectedAircraftChangedHookResult);
                    _this._SelectedAircraftChangedHookResult = null;
                }
                if (_this._ListUpdatedHookResult) {
                    _this._AnnounceSelectedAircraftList.unhook(_this._ListUpdatedHookResult);
                    _this._ListUpdatedHookResult = null;
                }
                _this._AnnounceSelectedAircraftList = null;
                _this._AutoplayQueue = [];
                _this.stopPlaying();
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                return $.extend(_this.createSettings(), savedSettings);
            };
            this.applyState = function (settings) {
                _this.setAnnounceOnlyAutoSelected(settings.announceOnlyAutoSelected);
                _this.setAnnounceSelected(settings.announceSelected);
                _this.setVolume(settings.volume);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var pane = new VRS.OptionPane({
                    name: 'vrsAudioPane',
                    titleKey: 'PaneAudio',
                    displayOrder: displayOrder
                });
                if (_this.canPlayAudio(true)) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'announceSelected',
                        labelKey: 'AnnounceSelected',
                        getValue: _this.getAnnounceSelected,
                        setValue: _this.setAnnounceSelected,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'announceOnlyAutoSelected',
                        labelKey: 'OnlyAutoSelected',
                        getValue: _this.getAnnounceOnlyAutoSelected,
                        setValue: _this.setAnnounceOnlyAutoSelected,
                        saveState: _this.saveState
                    }));
                }
                return pane;
            };
            this.isSupported = function () {
                if (_this._BrowserSupportsAudio === undefined) {
                    _this._BrowserSupportsAudio = false;
                    try {
                        var audioElement = new Audio();
                        _this._BrowserSupportsAudio = audioElement.play !== undefined;
                        if (_this._BrowserSupportsAudio) {
                            _this._BrowserSupportsAudio = audioElement.canPlayType !== undefined;
                        }
                        if (_this._BrowserSupportsAudio) {
                            _this._BrowserSupportsAudio = false;
                            var testCodecs = ['audio/x-wav;codecs=1', 'audio/wav;codecs=1'];
                            $.each(testCodecs, function (idx, codecAsObject) {
                                var canPlay = audioElement.canPlayType(String(codecAsObject));
                                _this._BrowserSupportsAudio = canPlay && canPlay !== 'no';
                                return !_this._BrowserSupportsAudio;
                            });
                        }
                    }
                    catch (e) {
                        _this._BrowserSupportsAudio = false;
                    }
                }
                return _this._BrowserSupportsAudio;
            };
            this.isAutoplaySupported = function () {
                return !VRS.browserHelper.isProbablyIPad() && !VRS.browserHelper.isProbablyIPhone();
            };
            this.canPlayAudio = function (mustAllowAutoPlay) {
                if (mustAllowAutoPlay === void 0) { mustAllowAutoPlay = true; }
                var result = VRS.globalOptions.audioEnabled;
                if (result)
                    result = _this.isSupported();
                if (result && mustAllowAutoPlay)
                    result = _this.isAutoplaySupported();
                if (result)
                    result = VRS.serverConfig ? VRS.serverConfig.audioEnabled() : true;
                return result;
            };
            this.playNextEntry = function () {
                if (!_this._Playing && _this._AutoplayQueue.length) {
                    var details = _this._AutoplayQueue[0];
                    _this._AutoplayQueue.splice(0, 1);
                    _this.playSource(details.src);
                }
            };
            this.playSource = function (source) {
                if (_this.canPlayAudio(false)) {
                    _this._Playing = $('<audio/>');
                    _this._Playing.on('ended', _this.audioEnded);
                    _this.stopAudioTimeout();
                    _this._PlayingTimeout = setTimeout(_this.audioTimedOut, VRS.globalOptions.audioTimeout);
                    var audioElement = (_this._Playing[0]);
                    audioElement.src = source;
                    audioElement.volume = _this.getVolume();
                    audioElement.muted = _this.getMuted();
                    audioElement.play();
                }
            };
            this.stopPlaying = function () {
                _this._AnnouncedAircraft = null;
                _this._AnnouncedAircraftIsUserSelected = undefined;
                _this.stopAudioTimeout();
                if (_this._Playing) {
                    var audio = _this._Playing;
                    _this._Playing = null;
                    (audio[0]).pause();
                    audio.attr('src', '');
                    audio.off();
                }
            };
            this.stopAudioTimeout = function () {
                if (_this._PlayingTimeout) {
                    clearTimeout(_this._PlayingTimeout);
                    _this._PlayingTimeout = 0;
                }
            };
            this.announceAircraft = function (aircraft, userSelected) {
                if (!aircraft)
                    throw 'aircraft is null or undefined';
                var newAircraft = _this._AnnouncedAircraft === null || _this._AnnouncedAircraft !== aircraft;
                var ignore = !_this.canPlayAudio(true);
                if (!ignore)
                    ignore = !_this._AnnounceSelected || (userSelected && _this._AnnounceOnlyAutoSelected);
                if (!ignore)
                    ignore = newAircraft && !userSelected && _this.isPreviouslyAnnouncedAutoSelected(aircraft);
                if (!ignore) {
                    if (newAircraft && !userSelected) {
                        _this.recordPreviouslyAnnouncedAutoSelected(aircraft);
                    }
                    var sayText = '';
                    var appendRoute = false;
                    var formatCallsign = function () {
                        appendRoute = true;
                        return VRS.stringUtility.format(VRS.$$.SayCallsign, _this.formatAcronymForSpeech(aircraft.formatCallsign(false))) + ' ';
                    };
                    var formatIcao = function () { return VRS.stringUtility.format(VRS.$$.SayIcao, _this.formatAcronymForSpeech(aircraft.formatIcao())); };
                    var formatModelIcao = function () { return VRS.stringUtility.format(VRS.$$.SayModelIcao, _this.formatUpperCaseWordsForSpeech(aircraft.formatModelIcao())) + ' '; };
                    var formatOperator = function () { return VRS.stringUtility.format(VRS.$$.SayOperator, _this.formatUpperCaseWordsForSpeech(aircraft.formatOperator())) + ' '; };
                    var formatRegistration = function () { return VRS.stringUtility.format(VRS.$$.SayRegistration, _this.formatAcronymForSpeech(aircraft.formatRegistration())) + ' '; };
                    if (aircraft === _this._AnnouncedAircraft) {
                        if (aircraft.registration.val && aircraft.registration.chg)
                            sayText += formatRegistration();
                        if (aircraft.callsign.val && aircraft.callsign.chg)
                            sayText += formatCallsign();
                    }
                    else if (newAircraft) {
                        if (aircraft.registration.val)
                            sayText += formatRegistration();
                        else
                            sayText += formatIcao();
                        if (aircraft.modelIcao.val)
                            sayText += formatModelIcao();
                        if (aircraft.operator.val)
                            sayText += formatOperator();
                        if (aircraft.callsign.val)
                            sayText += formatCallsign();
                    }
                    if (appendRoute) {
                        if (!aircraft.hasRoute()) {
                            sayText += VRS.$$.SayRouteNotKnown + ' ';
                        }
                        else {
                            var from = _this.formatUpperCaseWordsForSpeech(aircraft.from.val);
                            var to = _this.formatUpperCaseWordsForSpeech(aircraft.to.val);
                            var convertedVias = [];
                            var viaLength = aircraft.via.arr.length;
                            for (var i = 0; i < viaLength; ++i) {
                                convertedVias.push(_this.formatUpperCaseWordsForSpeech(aircraft.via.arr[i].val));
                            }
                            var via = convertedVias.length === 0 ? null : VRS.$$.sayStopovers(convertedVias);
                            if (!via)
                                sayText += VRS.stringUtility.format(VRS.$$.SayFromTo, from, to);
                            else
                                sayText += VRS.stringUtility.format(VRS.$$.SayFromToVia, from, via, to);
                        }
                    }
                    _this._AnnouncedAircraft = aircraft;
                    _this._AnnouncedAircraftIsUserSelected = userSelected;
                    if (newAircraft) {
                        _this._AutoplayQueue = [];
                        _this.stopPlaying();
                    }
                    if (sayText) {
                        _this.say(sayText);
                    }
                }
            };
            this.isPreviouslyAnnouncedAutoSelected = function (aircraft) {
                var result = false;
                var length = _this._PreviouslyAnnouncedAircraftIds.length;
                for (var i = 0; i < length; ++i) {
                    if (aircraft.id === _this._PreviouslyAnnouncedAircraftIds[i]) {
                        result = true;
                        break;
                    }
                }
                return result;
            };
            this.recordPreviouslyAnnouncedAutoSelected = function (aircraft) {
                if (_this._PreviouslyAnnouncedAircraftIds.length === 5) {
                    _this._PreviouslyAnnouncedAircraftIds.splice(0, 1);
                }
                _this._PreviouslyAnnouncedAircraftIds.push(aircraft.id);
            };
            this.say = function (text) {
                if (text && _this.canPlayAudio(true)) {
                    var details = { src: 'Audio?cmd=say&line=' + encodeURIComponent(text) };
                    _this._AutoplayQueue.push(details);
                    _this.playNextEntry();
                }
            };
            this.formatAcronymForSpeech = function (acronym) {
                var result = '';
                if (!acronym)
                    acronym = '';
                var length = acronym.length;
                for (var c = 0; c < length; c++) {
                    var ch = acronym[c];
                    switch (acronym[c]) {
                        case 'A':
                            ch = VRS.$$.SayAlpha;
                            break;
                        case 'B':
                            ch = VRS.$$.SayBravo;
                            break;
                        case 'C':
                            ch = VRS.$$.SayCharlie;
                            break;
                        case 'D':
                            ch = VRS.$$.SayDelta;
                            break;
                        case 'E':
                            ch = VRS.$$.SayEcho;
                            break;
                        case 'F':
                            ch = VRS.$$.SayFoxtrot;
                            break;
                        case 'G':
                            ch = VRS.$$.SayGolf;
                            break;
                        case 'H':
                            ch = VRS.$$.SayHotel;
                            break;
                        case 'I':
                            ch = VRS.$$.SayIndia;
                            break;
                        case 'J':
                            ch = VRS.$$.SayJuliet;
                            break;
                        case 'K':
                            ch = VRS.$$.SayKilo;
                            break;
                        case 'L':
                            ch = VRS.$$.SayLima;
                            break;
                        case 'M':
                            ch = VRS.$$.SayMike;
                            break;
                        case 'N':
                            ch = VRS.$$.SayNovember;
                            break;
                        case 'O':
                            ch = VRS.$$.SayOscar;
                            break;
                        case 'P':
                            ch = VRS.$$.SayPapa;
                            break;
                        case 'Q':
                            ch = VRS.$$.SayQuebec;
                            break;
                        case 'R':
                            ch = VRS.$$.SayRomeo;
                            break;
                        case 'S':
                            ch = VRS.$$.SaySierra;
                            break;
                        case 'T':
                            ch = VRS.$$.SayTango;
                            break;
                        case 'U':
                            ch = VRS.$$.SayUniform;
                            break;
                        case 'V':
                            ch = VRS.$$.SayVictor;
                            break;
                        case 'W':
                            ch = VRS.$$.SayWhiskey;
                            break;
                        case 'X':
                            ch = VRS.$$.SayXRay;
                            break;
                        case 'Y':
                            ch = VRS.$$.SayYankee;
                            break;
                        case 'Z':
                            ch = VRS.$$.SayZulu;
                            break;
                        case '-':
                            ch = VRS.$$.SayHyphen;
                            break;
                    }
                    if (ch !== ' ')
                        result += ch + _this._PausePhrase;
                }
                return result;
            };
            this.formatUpperCaseWordsForSpeech = function (text) {
                var result = '';
                text = text ? text : '';
                var chunks = text.split(' ');
                var length = chunks.length;
                for (var c = 0; c < length; c++) {
                    if (c > 0)
                        result += ' ';
                    var chunk = chunks[c];
                    result += VRS.stringUtility.isUpperCase(chunk) ? _this.formatPunctuationForSpeech(chunk) : chunk;
                }
                return result;
            };
            this.formatPunctuationForSpeech = function (text) {
                var result = '';
                text = text ? text : '';
                var length = text.length;
                for (var c = 0; c < length; c++) {
                    var ch = text[c];
                    switch (ch) {
                        case '-':
                            ch = VRS.$$.SayHyphen;
                            break;
                        case 'A':
                            ch = VRS.$$.SayAy;
                            break;
                        case 'Z':
                            ch = VRS.$$.SayZed;
                            break;
                    }
                    if (ch !== ' ') {
                        result += ch + _this._PausePhrase;
                    }
                }
                return result;
            };
            this.annouceSelectedAircraftOnList = function (aircraftList) {
                if (_this._SelectedAircraftChangedHookResult) {
                    _this._AnnounceSelectedAircraftList.unhook(_this._SelectedAircraftChangedHookResult);
                }
                if (_this._ListUpdatedHookResult) {
                    _this._AnnounceSelectedAircraftList.unhook(_this._ListUpdatedHookResult);
                }
                _this._AnnounceSelectedAircraftList = aircraftList;
                _this._SelectedAircraftChangedHookResult = aircraftList.hookSelectedAircraftChanged(_this.selectedAircraftChanged, _this);
                _this._ListUpdatedHookResult = aircraftList.hookUpdated(_this.listUpdated, _this);
            };
            this.audioEnded = function () {
                _this.stopAudioTimeout();
                _this._Playing.off();
                _this._Playing = null;
                _this.playNextEntry();
            };
            this.audioTimedOut = function () {
                _this._AutoplayQueue = [];
                _this.stopPlaying();
            };
            this.selectedAircraftChanged = function () {
                if (_this._AnnounceSelectedAircraftList) {
                    var selectedAircraft = _this._AnnounceSelectedAircraftList.getSelectedAircraft();
                    if (selectedAircraft) {
                        _this.announceAircraft(_this._AnnounceSelectedAircraftList.getSelectedAircraft(), _this._AnnounceSelectedAircraftList.getWasAircraftSelectedByUser());
                    }
                    else {
                        _this._AutoplayQueue = [];
                        _this.stopPlaying();
                    }
                }
            };
            this.listUpdated = function () {
                if (_this._AnnouncedAircraft) {
                    _this.announceAircraft(_this._AnnouncedAircraft, _this._AnnouncedAircraftIsUserSelected);
                }
            };
            this._Name = name || 'default';
        }
        AudioWrapper.prototype.persistenceKey = function () {
            return 'vrsAudio-' + this.getName();
        };
        AudioWrapper.prototype.createSettings = function () {
            return {
                announceSelected: this.getAnnounceSelected(),
                announceOnlyAutoSelected: this.getAnnounceOnlyAutoSelected(),
                volume: this.getVolume()
            };
        };
        return AudioWrapper;
    }());
    VRS.AudioWrapper = AudioWrapper;
})(VRS || (VRS = {}));
//# sourceMappingURL=audio.js.map