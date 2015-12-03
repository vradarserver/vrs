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
 * @fileoverview Code that deals with the audio features of the site.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.audioEnabled = VRS.globalOptions.audioEnabled !== undefined ? VRS.globalOptions.audioEnabled : true;                                                      // True if the audio features are enabled (can still be disabled on server). False if they are to be suppressed.
    VRS.globalOptions.audioAnnounceSelected = VRS.globalOptions.audioAnnounceSelected !== undefined ? VRS.globalOptions.audioAnnounceSelected : false;                          // True if details of selected aircraft should be announced.
    VRS.globalOptions.audioAnnounceOnlyAutoSelected = VRS.globalOptions.audioAnnounceOnlyAutoSelected !== undefined ? VRS.globalOptions.audioAnnounceOnlyAutoSelected : false;  // True if only auto-selected aircraft should have their details announced.
    VRS.globalOptions.audioDefaultVolume = VRS.globalOptions.audioDefaultVolume !== undefined ? VRS.globalOptions.audioDefaultVolume : 1.0;                                     // The default volume for the audio control. Range is 0.0 to 1.0.
    VRS.globalOptions.audioTimeout = VRS.globalOptions.audioTimeout || 60000;                                                                                                   // The number of milliseconds that must elapse before audio is timed-out.
    //endregion

    //region Audio
    VRS.Audio = function(name)
    {
        //region -- Fields
        var that = this;
        var _PausePhrase = ' ';

        /**
         * True if the browser supports audio, false if it does not. Undefined if it has not yet been tested.
         * @type {boolean=}
         */
        var _BrowserSupportsAudio;

        /**
         * A queue of objects that describe URLs to automatically play.
         * @type {Array.<VRS_AUDIO_AUTOPLAY>}
         * @private
         */
        var _AutoplayQueue = [];

        /**
         * A jQuery object wrapping an HTML5 Audio object that is currently playing.
         * @type {Object}
         * @private
         */
        var _Playing = null;

        /**
         * The timeout handle for the timer that, when it expires, will stop the current audio object as a failsafe
         * against audio objects that are pointing to an invalid / slow source.
         * @type {number}
         * @private
         */
        var _PlayingTimeout = 0;

        /**
         * The aircraft list that we're reading out details for.
         * @type {VRS.AircraftList}
         * @private
         */
        var _AnnounceSelectedAircraftList = null;

        /**
         * The hook result for the selected aircraft changed event on an aircraft list.
         * @type {Object}
         * @private
         */
        var _SelectedAircraftChangedHookResult = null;

        /**
         * The hook result for the list updated event on an aircraft list.
         * @type {Object}
         * @private
         */
        var _ListUpdatedHookResult = null;

        /**
         * The aircraft whose details are currently being read out.
         * @type {VRS.Aircraft}
         * @private
         */
        var _AnnouncedAircraft = null;

        /**
         * True if the aircraft that's been announced was selected by the user, false if it was auto-selected.
         * @type {boolean}
         */
        var _AnnouncedAircraftIsUserSelected;

        /**
         * A collection of aircraft IDs whose details were previously read out. We record this to guard against a
         * ping-pong effect where the auto-selector rapidly switches between the same bunch of aircraft.
         * @type {Array.<number>}
         * @private
         */
        var _PreviouslyAnnouncedAircraftIds = [];
        //endregion

        //region -- Properties
        /** @type {String} */
        var _Name = name || 'default';
        this.getName = function() { return _Name; };

        /** @type {Boolean} */
        var _AnnounceSelected = VRS.globalOptions.audioAnnounceSelected;
        this.getAnnounceSelected = function() { return _AnnounceSelected; };
        this.setAnnounceSelected = function(/** Boolean */ value) {
            if(_AnnounceSelected !== value) {
                _AnnounceSelected = value;
                if(!_AnnounceSelected) {
                    _AutoplayQueue = [];
                    stopPlaying();
                }
            }
        };

        /** @type {Boolean} */
        var _AnnounceOnlyAutoSelected = VRS.globalOptions.audioAnnounceOnlyAutoSelected;
        this.getAnnounceOnlyAutoSelected = function() { return _AnnounceOnlyAutoSelected; };
        this.setAnnounceOnlyAutoSelected = function(/** Boolean */ value) {
            if(_AnnounceOnlyAutoSelected !== value) {
                _AnnounceOnlyAutoSelected = value;
                if(_AnnounceSelected && _AnnounceOnlyAutoSelected && _AnnouncedAircraftIsUserSelected) {
                    _AutoplayQueue = [];
                    stopPlaying();
                }
            }
        };

        /** @type {Number} */
        var _Volume = VRS.globalOptions.audioDefaultVolume;
        this.getVolume = function() { return _Volume; };
        this.setVolume = function(/** Number */ value) {
            if(_Volume !== value) {
                _Volume = value;
                if(_Playing) _Playing[0].volume = _Volume;
            }
        };

        /** @type {boolean} */
        var _Muted = false;
        this.getMuted = function() { return _Muted; };
        this.setMuted = function(/** boolean */ value) {
            if(_Muted !== value) {
                _Muted = value;
                if(_Playing) _Playing[0].muted = _Muted;
            }
        };
        //endregion

        //region -- dispose
        /**
         * Releases all resources held by the object.
         */
        this.dispose = function()
        {
            if(_SelectedAircraftChangedHookResult) {
                _AnnounceSelectedAircraftList.unhook(_SelectedAircraftChangedHookResult);
                _SelectedAircraftChangedHookResult = null;
            }
            if(_ListUpdatedHookResult) {
                _AnnounceSelectedAircraftList.unhook(_ListUpdatedHookResult);
                _ListUpdatedHookResult = null;
            }
            _AnnounceSelectedAircraftList = null;

            _AutoplayQueue = [];
            stopPlaying();
        };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_AUDIO}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_AUDIO} settings
         */
        this.applyState = function(settings)
        {
            that.setAnnounceOnlyAutoSelected(settings.announceOnlyAutoSelected);
            that.setAnnounceSelected(settings.announceSelected);
            that.setVolume(settings.volume);
        };

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key under which the state will be saved.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsAudio-' + that.getName();
        }

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_AUDIO}
         */
        function createSettings()
        {
            return {
                announceSelected:           that.getAnnounceSelected(),
                announceOnlyAutoSelected:   that.getAnnounceOnlyAutoSelected(),
                volume:                     that.getVolume()
            };
        }
        //endregion

        //region -- Configuration - createOptionPane
        /**
         * Creates the description of the options pane for the object.
         * @param {number} displayOrder
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAudioPane',
                titleKey:       'PaneAudio',
                displayOrder:   displayOrder
            });

            if(that.canPlayAudio(true)) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'announceSelected',
                    labelKey:       'AnnounceSelected',
                    getValue:       that.getAnnounceSelected,
                    setValue:       that.setAnnounceSelected,
                    saveState:      that.saveState
                }));
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'announceOnlyAutoSelected',
                    labelKey:       'OnlyAutoSelected',
                    getValue:       that.getAnnounceOnlyAutoSelected,
                    setValue:       that.setAnnounceOnlyAutoSelected,
                    saveState:      that.saveState
                }));
            }

            return pane;
        };
        //endregion

        //region -- isSupported, isAutoplaySupported, canPlayAudio
        /**
         * Returns true if the browser supports audio and the codecs we want to use, false if it does not.
         * @returns {boolean}
         */
        this.isSupported = function()
        {
            if(_BrowserSupportsAudio === undefined) {
                _BrowserSupportsAudio = false;
                try {
                    //noinspection JSUnresolvedFunction
                    var audioElement = new Audio();
                    _BrowserSupportsAudio = audioElement.play !== undefined;
                    if(_BrowserSupportsAudio) _BrowserSupportsAudio = audioElement.canPlayType !== undefined;
                    if(_BrowserSupportsAudio) {
                        _BrowserSupportsAudio = false;
                        var testCodecs = [ 'audio/x-wav;codecs=1', 'audio/wav;codecs=1' ];
                        $.each(testCodecs, function(/** number */ idx, /** Object */ codecAsObject) {
                            var canPlay = audioElement.canPlayType(String(codecAsObject));
                            _BrowserSupportsAudio = canPlay && canPlay !== 'no';
                            return !_BrowserSupportsAudio;
                        });
                    }
                } catch(e) {
                    _BrowserSupportsAudio = false;
                }
            }

            return _BrowserSupportsAudio;
        };

        /**
         * Returns true if the browser probably supports auto-play, false if it probably does not.
         * @returns {boolean}
         */
        this.isAutoplaySupported = function()
        {
            return !VRS.browserHelper.isProbablyIPad() && !VRS.browserHelper.isProbablyIPhone();
        };

        /**
         * Returns true if the browser allows audio and if all of the settings together allow audio. This checks everything -
         * that the global options allow it, that the browser supports it, optionally that the browser can auto-play and
         * that the server configuration allows it.
         * @param {boolean=} mustAllowAutoPlay  True if the browser must support auto-play, false to skip this test. Defaults to true.
         * @returns {boolean} True if all of the stars are lining up for us, false if audio must be suppressed.
         */
        this.canPlayAudio = function(mustAllowAutoPlay)
        {
            mustAllowAutoPlay = mustAllowAutoPlay === undefined ? true : !!mustAllowAutoPlay;

            var result = VRS.globalOptions.audioEnabled;
            if(result) result = that.isSupported();
            if(result && mustAllowAutoPlay) result = that.isAutoplaySupported();
            if(result) result = VRS.serverConfig ? VRS.serverConfig.audioEnabled() : true;

            return result;
        };
        //endregion

        //region -- playNextEntry, playSource, stopPlaying, stopAudioTimeout
        function playNextEntry()
        {
            if(!_Playing && _AutoplayQueue.length) {
                var details = _AutoplayQueue[0];
                _AutoplayQueue.splice(0, 1);
                playSource(details.src);
            }
        }

        /**
         * Plays any URL as an audio source.
         * @param {string} source
         */
        function playSource(source)
        {
            if(that.canPlayAudio(false)) {
                _Playing = $('<audio/>');
                _Playing.on('ended', audioEnded);

                stopAudioTimeout();
                _PlayingTimeout = setTimeout(audioTimedOut, VRS.globalOptions.audioTimeout);

                _Playing[0].src = source;
                _Playing[0].volume = that.getVolume();
                _Playing[0].muted = that.getMuted();
                _Playing[0].play();
            }
        }

        /**
         * Stops the currently playing audio object and releases it.
         */
        function stopPlaying()
        {
            _AnnouncedAircraft = null;
            _AnnouncedAircraftIsUserSelected = undefined;

            stopAudioTimeout();
            if(_Playing) {
                var audio = _Playing;
                _Playing = null;

                audio[0].pause();   // <-- this may trigger audioEnded()
                audio.attr('src', '');
                audio.off();
            }
        }

        /**
         * Stops the timer that's running against the currently-playing audio object.
         */
        function stopAudioTimeout()
        {
            if(_PlayingTimeout) {
                clearTimeout(_PlayingTimeout);
                _PlayingTimeout = 0;
            }
        }
        //endregion

        //region -- announceAircraft, isPreviouslyAnnouncedAutoSelected, recordPreviouslyAnnouncedAutoSelected
        /**
         * Calls out the details of the aircraft passed across if the settings allow it.
         * @param {VRS.Aircraft} aircraft           The aircraft to read out details for.
         * @param {boolean}      userSelected       True if the user selected the aircraft, false if it was auto-selected.
         */
        function announceAircraft(aircraft, userSelected)
        {
            if(!aircraft) throw 'aircraft is null or undefined';

            var newAircraft = _AnnouncedAircraft === null || _AnnouncedAircraft !== aircraft;
            var ignore = !that.canPlayAudio(true);
            if(!ignore) ignore = !_AnnounceSelected || (userSelected && _AnnounceOnlyAutoSelected);
            if(!ignore) ignore = newAircraft && !userSelected && isPreviouslyAnnouncedAutoSelected(aircraft);

            if(!ignore) {
                if(newAircraft && !userSelected) recordPreviouslyAnnouncedAutoSelected(aircraft);

                var sayText = '';
                var appendRoute = false;

                var formatCallsign = function() {
                    appendRoute = true;
                    return VRS.stringUtility.format(VRS.$$.SayCallsign, that.formatAcronymForSpeech(aircraft.formatCallsign(false))) + ' ';
                };
                var formatIcao = function() { return VRS.stringUtility.format(VRS.$$.SayIcao, that.formatAcronymForSpeech(aircraft.formatIcao())); };
                var formatModelIcao = function() { return VRS.stringUtility.format(VRS.$$.SayModelIcao, that.formatUpperCaseWordsForSpeech(aircraft.formatModelIcao())) + ' '; };
                var formatOperator = function() { return VRS.stringUtility.format(VRS.$$.SayOperator, that.formatUpperCaseWordsForSpeech(aircraft.formatOperator())) + ' '; };
                var formatRegistration = function() { return VRS.stringUtility.format(VRS.$$.SayRegistration, that.formatAcronymForSpeech(aircraft.formatRegistration())) + ' '; };


                if(aircraft === _AnnouncedAircraft) {
                    if(aircraft.registration.val && aircraft.registration.chg)  sayText += formatRegistration();
                    if(aircraft.callsign.val && aircraft.callsign.chg)          sayText += formatCallsign();
                } else if(newAircraft) {
                    if(aircraft.registration.val)   sayText += formatRegistration();
                    else                            sayText += formatIcao();
                    if(aircraft.modelIcao.val)      sayText += formatModelIcao();
                    if(aircraft.operator.val)       sayText += formatOperator();
                    if(aircraft.callsign.val)       sayText += formatCallsign();
                }

                if(appendRoute) {
                    if(!aircraft.hasRoute()) sayText += VRS.$$.SayRouteNotKnown + ' ';
                    else {
                        var from = that.formatUpperCaseWordsForSpeech(aircraft.from.val);
                        var to = that.formatUpperCaseWordsForSpeech(aircraft.to.val);
                        var convertedVias = [];
                        var viaLength = aircraft.via.arr.length;
                        for(var i = 0;i < viaLength;++i) {
                            convertedVias.push(that.formatUpperCaseWordsForSpeech(aircraft.via.arr[i].val));
                        }
                        var via = convertedVias.length === 0 ? null : VRS.$$.sayStopovers(convertedVias);

                        if(!via) sayText += VRS.stringUtility.format(VRS.$$.SayFromTo, from, to);
                        else     sayText += VRS.stringUtility.format(VRS.$$.SayFromToVia, from, via, to);
                    }
                }

                _AnnouncedAircraft = aircraft;
                _AnnouncedAircraftIsUserSelected = userSelected;
                if(newAircraft) {
                    _AutoplayQueue = [];
                    stopPlaying();
                }
                if(sayText) that.say(sayText);
            }
        }

        /**
         * Returns true if the aircraft has been read out previously.
         * @param {VRS.Aircraft} aircraft
         * @returns {boolean}
         */
        function isPreviouslyAnnouncedAutoSelected(aircraft)
        {
            var result = false;
            var length = _PreviouslyAnnouncedAircraftIds.length;
            for(var i = 0;i < length;++i) {
                if(aircraft.id === _PreviouslyAnnouncedAircraftIds[i]) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /**
         * Records the auto-selected aircraft's details.
         * @param {VRS.Aircraft} aircraft
         */
        function recordPreviouslyAnnouncedAutoSelected(aircraft)
        {
            if(_PreviouslyAnnouncedAircraftIds.length === 5) _PreviouslyAnnouncedAircraftIds.splice(0, 1);
            _PreviouslyAnnouncedAircraftIds.push(aircraft.id);
        }
        //endregion

        //region -- say, formatAcronymForSpeech, formatUpperCaseWordsForSpeech, formatPunctuationForSpeech
        /**
         * Queues up a text-to-speech playback of the text passed across. Note that the text is played 'as-is' - there
         * is no attempt to convert upper-case to the phonetic alphabet or anything like that.
         * @param {string} text The text to play back.
         */
        this.say = function(text)
        {
            if(text && that.canPlayAudio(true)) {
                var details = { src: 'Audio?cmd=say&line=' + encodeURIComponent(text) };
                _AutoplayQueue.push(details);
                playNextEntry();
            }
        };

        /**
         * Takes an acronym (e.g. 'LHR') and returns the text that should cause the acronym to be spoken using the
         * phonetic alphabet.
         * @param {string} acronym
         * @returns {string}
         */
        this.formatAcronymForSpeech = function(acronym)
        {
            var result = '';

            if(!acronym) acronym = '';
            var length = acronym.length;
            for(var c = 0;c < length;c++) {
                var ch = acronym[c];
                switch(acronym[c]) {
                    case 'A':   ch = VRS.$$.SayAlpha; break;
                    case 'B':   ch = VRS.$$.SayBravo; break;
                    case 'C':   ch = VRS.$$.SayCharlie; break;
                    case 'D':   ch = VRS.$$.SayDelta; break;
                    case 'E':   ch = VRS.$$.SayEcho; break;
                    case 'F':   ch = VRS.$$.SayFoxtrot; break;
                    case 'G':   ch = VRS.$$.SayGolf; break;
                    case 'H':   ch = VRS.$$.SayHotel; break;
                    case 'I':   ch = VRS.$$.SayIndia; break;
                    case 'J':   ch = VRS.$$.SayJuliet; break;
                    case 'K':   ch = VRS.$$.SayKilo; break;
                    case 'L':   ch = VRS.$$.SayLima; break;
                    case 'M':   ch = VRS.$$.SayMike; break;
                    case 'N':   ch = VRS.$$.SayNovember; break;
                    case 'O':   ch = VRS.$$.SayOscar; break;
                    case 'P':   ch = VRS.$$.SayPapa; break;
                    case 'Q':   ch = VRS.$$.SayQuebec; break;
                    case 'R':   ch = VRS.$$.SayRomeo; break;
                    case 'S':   ch = VRS.$$.SaySierra; break;
                    case 'T':   ch = VRS.$$.SayTango; break;
                    case 'U':   ch = VRS.$$.SayUniform; break;
                    case 'V':   ch = VRS.$$.SayVictor; break;
                    case 'W':   ch = VRS.$$.SayWhiskey; break;
                    case 'X':   ch = VRS.$$.SayXRay; break;
                    case 'Y':   ch = VRS.$$.SayYankee; break;
                    case 'Z':   ch = VRS.$$.SayZulu; break;
                    case '-':   ch = VRS.$$.SayHyphen; break;
                }
                if(ch !== ' ') result += ch + _PausePhrase;
            }

            return result;
        };

        /**
         * Returns the text passed in with any upper-case words (e.g. 'LHR' in 'LHR London Heathrow') transformed so
         * that text-to-speech synthesis will say each individual letter in the word rather than trying to pronounce
         * the letters as a single word (e.g. LAX would be pronounced as ELL-AY-EX rather than LAX).
         * @param {string} text
         * @returns {string}
         */
        this.formatUpperCaseWordsForSpeech = function(text)
        {
            var result = '';
            text = text ? text : '';
            var chunks = text.split(' ');
            var length = chunks.length;
            for(var c = 0;c < length;c++) {
                if(c > 0) result += ' ';
                var chunk = chunks[c];
                result += VRS.stringUtility.isUpperCase(chunk) ? that.formatPunctuationForSpeech(chunk) : chunk;
            }

            return result;
        };

        /**
         * Returns a string that will cause the text-to-speech synthesis to spell out each character in the text
         * rather than pronouncing the text as a word. E.G. if passed 'HI' it will say 'aitch-ey' rather than 'hi'.
         * @param {string} text
         * @returns {string}
         */
        this.formatPunctuationForSpeech = function(text)
        {
            var result = '';
            text = text ? text : '';
            var length = text.length;
            for(var c = 0;c < length;c++) {
                var ch = text[c];
                if(ch === '-') ch = 'hyphen';
                if(ch === 'Z') ch = 'ZED';
                if(ch !== ' ') result += ch + _PausePhrase;
            }

            return result;
        };
        //endregion

        //region -- annouceSelectedAircraftOnList
        /**
         * Attaches event handlers to the aircraft list such that when a new aircraft is selected it will read out
         * details about the aircraft, if the configuration options at the time allow it.
         * @param {VRS.AircraftList} aircraftList
         */
        this.annouceSelectedAircraftOnList = function(aircraftList)
        {
            if(_SelectedAircraftChangedHookResult) _AnnounceSelectedAircraftList.unhook(_SelectedAircraftChangedHookResult);
            if(_ListUpdatedHookResult)             _AnnounceSelectedAircraftList.unhook(_ListUpdatedHookResult);
            _AnnounceSelectedAircraftList = aircraftList;
            _SelectedAircraftChangedHookResult = aircraftList.hookSelectedAircraftChanged(selectedAircraftChanged, this);
            _ListUpdatedHookResult = aircraftList.hookUpdated(listUpdated, this);
        };
        //endregion

        //region -- Events subscribed
        /**
         * Called when the currently-playing audio object has finished playing its audio.
         */
        function audioEnded()
        {
            stopAudioTimeout();

            _Playing.off();
            _Playing = null;
            playNextEntry();
        }

        /**
         * Called after the audio object has existed for a few seconds. It's assumed that if this gets called then the
         * audio object is pointing at a bad source and should be stopped. If one source is bad then all are considered
         * bad and the queue is cleared down.
         */
        function audioTimedOut()
        {
            _AutoplayQueue = [];
            stopPlaying();
        }

        /**
         * Called when an aircraft list changes the selected aircraft.
         //* @param {VRS.Aircraft} previouslySelectedAircraft
         */
        function selectedAircraftChanged()
        {
            if(_AnnounceSelectedAircraftList) {
                var selectedAircraft = _AnnounceSelectedAircraftList.getSelectedAircraft();
                if(selectedAircraft) announceAircraft(_AnnounceSelectedAircraftList.getSelectedAircraft(), _AnnounceSelectedAircraftList.getWasAircraftSelectedByUser());
                else {
                    _AutoplayQueue = [];
                    stopPlaying();
                }
            }
        }

        /**
         * Called when the aircraft list is updated.
         */
        function listUpdated()
        {
            // Re-announce the aircraft, which should call out any new details.
            if(_AnnouncedAircraft) announceAircraft(_AnnouncedAircraft, _AnnouncedAircraftIsUserSelected);
        }
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
