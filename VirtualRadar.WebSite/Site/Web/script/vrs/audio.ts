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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.audioEnabled = VRS.globalOptions.audioEnabled !== undefined ? VRS.globalOptions.audioEnabled : true;                                                      // True if the audio features are enabled (can still be disabled on server). False if they are to be suppressed.
    VRS.globalOptions.audioAnnounceSelected = VRS.globalOptions.audioAnnounceSelected !== undefined ? VRS.globalOptions.audioAnnounceSelected : false;                          // True if details of selected aircraft should be announced.
    VRS.globalOptions.audioAnnounceOnlyAutoSelected = VRS.globalOptions.audioAnnounceOnlyAutoSelected !== undefined ? VRS.globalOptions.audioAnnounceOnlyAutoSelected : false;  // True if only auto-selected aircraft should have their details announced.
    VRS.globalOptions.audioDefaultVolume = VRS.globalOptions.audioDefaultVolume !== undefined ? VRS.globalOptions.audioDefaultVolume : 1.0;                                     // The default volume for the audio control. Range is 0.0 to 1.0.
    VRS.globalOptions.audioTimeout = VRS.globalOptions.audioTimeout || 60000;                                                                                                   // The number of milliseconds that must elapse before audio is timed-out.

    /**
     * Describes URLs that the audio class will automatically play.
     */
    export interface Audio_AutoPlay
    {
        src: string;
    }

    /**
     * The settings that are persisted between sessions by the Audio object.
     */
    export interface AudioWrapper_SaveState
    {
        announceSelected:           boolean;
        announceOnlyAutoSelected:   boolean;
        volume:                     number;
    }

    /**
     * Handles the audio callouts for the site.
     */
    export class AudioWrapper implements ISelfPersist<AudioWrapper_SaveState>
    {
        private _PausePhrase = ' ';
        private _BrowserSupportsAudio: boolean;                             // True if the browser supports audio, false if it does not. Undefined if it has not yet been tested.
        private _AutoplayQueue: Audio_AutoPlay[] = [];                      // A queue of objects that describe URLs to automatically play.
        private _Playing: JQuery = null;                                    // A jQuery object wrapping an HTML5 Audio object that is currently playing.
        private _PlayingTimeout = 0;                                        // The timeout handle for the timer that, when it expires, will stop the current audio object as a failsafe against audio objects that are pointing to an invalid / slow source.
        private _AnnounceSelectedAircraftList: AircraftList = null;         // The aircraft list that we're reading out details for.
        private _SelectedAircraftChangedHookResult: IEventHandle = null;    // The hook result for the selected aircraft changed event on an aircraft list.
        private _ListUpdatedHookResult : IEventHandle = null;               // The hook result for the list updated event on an aircraft list.
        private _AnnouncedAircraft: Aircraft = null;                        // The aircraft whose details are currently being read out.
        private _AnnouncedAircraftIsUserSelected;                           // True if the aircraft that's been announced was selected by the user, false if it was auto-selected.
        private _PreviouslyAnnouncedAircraftIds: number[] = [];             // A collection of aircraft IDs whose details were previously read out. We record this to guard against a ping-pong effect where the auto-selector rapidly switches between the same bunch of aircraft.

        private _Name: string;
        private _AnnounceSelected: boolean = VRS.globalOptions.audioAnnounceSelected;
        private _AnnounceOnlyAutoSelected: boolean = VRS.globalOptions.audioAnnounceOnlyAutoSelected;
        private _Volume: number = VRS.globalOptions.audioDefaultVolume;
        private _Muted = false;

        constructor(name?: string)
        {
            this._Name = name || 'default';
        }

        getName = () : string =>
        {
            return this._Name;
        }

        getAnnounceSelected = () =>
        {
            return this._AnnounceSelected;
        }
        setAnnounceSelected = (value: boolean) =>
        {
            if(this._AnnounceSelected !== value) {
                this._AnnounceSelected = value;
                if(!this._AnnounceSelected) {
                    this._AutoplayQueue = [];
                    this.stopPlaying();
                }
            }
        }

        getAnnounceOnlyAutoSelected = () =>
        {
            return this._AnnounceOnlyAutoSelected;
        }
        setAnnounceOnlyAutoSelected = (value: boolean) =>
        {
            if(this._AnnounceOnlyAutoSelected !== value) {
                this._AnnounceOnlyAutoSelected = value;
                if(this._AnnounceSelected && this._AnnounceOnlyAutoSelected && this._AnnouncedAircraftIsUserSelected) {
                    this._AutoplayQueue = [];
                    this.stopPlaying();
                }
            }
        }

        getVolume = () =>
        {
            return this._Volume;
        }
        setVolume = (value: number) =>
        {
            if(this._Volume !== value) {
                this._Volume = value;
                if(this._Playing) {
                    (<HTMLAudioElement>(this._Playing[0])).volume = this._Volume;
                }
            }
        }

        getMuted = () =>
        {
            return this._Muted;
        }
        setMuted = (value: boolean) =>
        {
            if(this._Muted !== value) {
                this._Muted = value;
                if(this._Playing) {
                    (<HTMLAudioElement>(this._Playing[0])).muted = this._Muted;
                }
            }
        }

        /**
         * Releases all resources held by the object.
         */
        dispose = () =>
        {
            if(this._SelectedAircraftChangedHookResult) {
                this._AnnounceSelectedAircraftList.unhook(this._SelectedAircraftChangedHookResult);
                this._SelectedAircraftChangedHookResult = null;
            }
            if(this._ListUpdatedHookResult) {
                this._AnnounceSelectedAircraftList.unhook(this._ListUpdatedHookResult);
                this._ListUpdatedHookResult = null;
            }
            this._AnnounceSelectedAircraftList = null;

            this._AutoplayQueue = [];
            this.stopPlaying();
        }

        /**
         * Saves the current state of the object.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState = () : AudioWrapper_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState = (settings: AudioWrapper_SaveState) =>
        {
            this.setAnnounceOnlyAutoSelected(settings.announceOnlyAutoSelected);
            this.setAnnounceSelected(settings.announceSelected);
            this.setVolume(settings.volume);
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey() : string
        {
            return 'vrsAudio-' + this.getName();
        }

        /**
         * Creates the saved state object.
         */
        private createSettings() : AudioWrapper_SaveState
        {
            return {
                announceSelected:           this.getAnnounceSelected(),
                announceOnlyAutoSelected:   this.getAnnounceOnlyAutoSelected(),
                volume:                     this.getVolume()
            };
        }

        /**
         * Creates the description of the options pane for the object.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAudioPane',
                titleKey:       'PaneAudio',
                displayOrder:   displayOrder
            });

            if(this.canPlayAudio(true)) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'announceSelected',
                    labelKey:       'AnnounceSelected',
                    getValue:       this.getAnnounceSelected,
                    setValue:       this.setAnnounceSelected,
                    saveState:      this.saveState
                }));
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'announceOnlyAutoSelected',
                    labelKey:       'OnlyAutoSelected',
                    getValue:       this.getAnnounceOnlyAutoSelected,
                    setValue:       this.setAnnounceOnlyAutoSelected,
                    saveState:      this.saveState
                }));
            }

            return pane;
        }

        /**
         * Returns true if the browser supports audio and the codecs we want to use, false if it does not.
         */
        isSupported = () : boolean =>
        {
            if(this._BrowserSupportsAudio === undefined) {
                this._BrowserSupportsAudio = false;
                try {
                    var audioElement = new Audio();
                    this._BrowserSupportsAudio = audioElement.play !== undefined;
                    if(this._BrowserSupportsAudio) {
                        this._BrowserSupportsAudio = audioElement.canPlayType !== undefined;
                    }
                    if(this._BrowserSupportsAudio) {
                        this._BrowserSupportsAudio = false;
                        var testCodecs = [ 'audio/x-wav;codecs=1', 'audio/wav;codecs=1' ];
                        $.each(testCodecs, (idx, codecAsObject) => {
                            var canPlay = audioElement.canPlayType(String(codecAsObject));
                            this._BrowserSupportsAudio = canPlay && canPlay !== 'no';
                            return !this._BrowserSupportsAudio;
                        });
                    }
                } catch(e) {
                    this._BrowserSupportsAudio = false;
                }
            }

            return this._BrowserSupportsAudio;
        }

        /**
         * Returns true if the browser probably supports auto-play, false if it probably does not.
         */
        isAutoplaySupported = () : boolean =>
        {
            return !VRS.browserHelper.isProbablyIPad() && !VRS.browserHelper.isProbablyIPhone();
        }

        /**
         * Returns true if the browser allows audio and if all of the settings together allow audio. This checks everything -
         * that the global options allow it, that the browser supports it, optionally that the browser can auto-play and
         * that the server configuration allows it.
         */
        canPlayAudio = (mustAllowAutoPlay: boolean = true) : boolean =>
        {
            var result = VRS.globalOptions.audioEnabled;
            if(result) result = this.isSupported();
            if(result && mustAllowAutoPlay) result = this.isAutoplaySupported();
            if(result) result = VRS.serverConfig ? VRS.serverConfig.audioEnabled() : true;

            return result;
        }

        private playNextEntry = () =>
        {
            if(!this._Playing && this._AutoplayQueue.length) {
                var details = this._AutoplayQueue[0];
                this._AutoplayQueue.splice(0, 1);
                this.playSource(details.src);
            }
        }

        /**
         * Plays any URL as an audio source.
         */
        private playSource = (source: string) =>
        {
            if(this.canPlayAudio(false)) {
                this._Playing = $('<audio/>');
                this._Playing.on('ended', this.audioEnded);

                this.stopAudioTimeout();
                this._PlayingTimeout = setTimeout(this.audioTimedOut, VRS.globalOptions.audioTimeout);

                var audioElement = (<HTMLAudioElement>(this._Playing[0]));
                audioElement.src = source;
                audioElement.volume = this.getVolume();
                audioElement.muted = this.getMuted();
                audioElement.play();
            }
        }

        /**
         * Stops the currently playing audio object and releases it.
         */
        private stopPlaying = () =>
        {
            this._AnnouncedAircraft = null;
            this._AnnouncedAircraftIsUserSelected = undefined;

            this.stopAudioTimeout();
            if(this._Playing) {
                var audio = this._Playing;
                this._Playing = null;

                (<HTMLAudioElement>(audio[0])).pause();   // <-- this may trigger audioEnded()
                audio.attr('src', '');
                audio.off();
            }
        }

        /**
         * Stops the timer that's running against the currently-playing audio object.
         */
        private stopAudioTimeout = () =>
        {
            if(this._PlayingTimeout) {
                clearTimeout(this._PlayingTimeout);
                this._PlayingTimeout = 0;
            }
        }

        /**
         * Calls out the details of the aircraft passed across if the settings allow it.
         */
        private announceAircraft = (aircraft: Aircraft, userSelected: boolean) =>
        {
            if(!aircraft) throw 'aircraft is null or undefined';

            var newAircraft = this._AnnouncedAircraft === null || this._AnnouncedAircraft !== aircraft;
            var ignore = !this.canPlayAudio(true);
            if(!ignore) ignore = !this._AnnounceSelected || (userSelected && this._AnnounceOnlyAutoSelected);
            if(!ignore) ignore = newAircraft && !userSelected && this.isPreviouslyAnnouncedAutoSelected(aircraft);

            if(!ignore) {
                if(newAircraft && !userSelected) {
                    this.recordPreviouslyAnnouncedAutoSelected(aircraft);
                }

                var sayText = '';
                var appendRoute = false;

                var formatCallsign = () => {
                    appendRoute = true;
                    return VRS.stringUtility.format(VRS.$$.SayCallsign, this.formatAcronymForSpeech(aircraft.formatCallsign(false))) + ' ';
                };
                var formatIcao = () => VRS.stringUtility.format(VRS.$$.SayIcao, this.formatAcronymForSpeech(aircraft.formatIcao()));
                var formatModelIcao = () => VRS.stringUtility.format(VRS.$$.SayModelIcao, this.formatUpperCaseWordsForSpeech(aircraft.formatModelIcao())) + ' ';
                var formatOperator = () => VRS.stringUtility.format(VRS.$$.SayOperator, this.formatUpperCaseWordsForSpeech(aircraft.formatOperator())) + ' ';
                var formatRegistration = () => VRS.stringUtility.format(VRS.$$.SayRegistration, this.formatAcronymForSpeech(aircraft.formatRegistration())) + ' ';


                if(aircraft === this._AnnouncedAircraft) {
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
                    if(!aircraft.hasRoute()) {
                        sayText += VRS.$$.SayRouteNotKnown + ' ';
                    } else {
                        var from = this.formatUpperCaseWordsForSpeech(aircraft.from.val);
                        var to = this.formatUpperCaseWordsForSpeech(aircraft.to.val);
                        var convertedVias = [];
                        var viaLength = aircraft.via.arr.length;
                        for(var i = 0;i < viaLength;++i) {
                            convertedVias.push(this.formatUpperCaseWordsForSpeech(aircraft.via.arr[i].val));
                        }
                        var via = convertedVias.length === 0 ? null : VRS.$$.sayStopovers(convertedVias);

                        if(!via) sayText += VRS.stringUtility.format(VRS.$$.SayFromTo, from, to);
                        else     sayText += VRS.stringUtility.format(VRS.$$.SayFromToVia, from, via, to);
                    }
                }

                this._AnnouncedAircraft = aircraft;
                this._AnnouncedAircraftIsUserSelected = userSelected;
                if(newAircraft) {
                    this._AutoplayQueue = [];
                    this.stopPlaying();
                }
                if(sayText) {
                    this.say(sayText);
                }
            }
        }

        /**
         * Returns true if the aircraft has been read out previously.
         */
        private isPreviouslyAnnouncedAutoSelected = (aircraft: Aircraft) : boolean =>
        {
            var result = false;
            var length = this._PreviouslyAnnouncedAircraftIds.length;
            for(var i = 0;i < length;++i) {
                if(aircraft.id === this._PreviouslyAnnouncedAircraftIds[i]) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /**
         * Records the auto-selected aircraft's details.
         */
        private recordPreviouslyAnnouncedAutoSelected = (aircraft: Aircraft) =>
        {
            if(this._PreviouslyAnnouncedAircraftIds.length === 5) {
                this._PreviouslyAnnouncedAircraftIds.splice(0, 1);
            }
            this._PreviouslyAnnouncedAircraftIds.push(aircraft.id);
        }

        /**
         * Queues up a text-to-speech playback of the text passed across. Note that the text is played 'as-is' - there
         * is no attempt to convert upper-case to the phonetic alphabet or anything like that.
         */
        say = (text: string) =>
        {
            if(text && this.canPlayAudio(true)) {
                var details = { src: 'Audio?cmd=say&line=' + encodeURIComponent(text) };
                this._AutoplayQueue.push(details);
                this.playNextEntry();
            }
        }

        /**
         * Takes an acronym (e.g. 'LHR') and returns the text that should cause the acronym to be spoken using the
         * phonetic alphabet.
         */
        formatAcronymForSpeech = (acronym: string) : string =>
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
                if(ch !== ' ') result += ch + this._PausePhrase;
            }

            return result;
        }

        /**
         * Returns the text passed in with any upper-case words (e.g. 'LHR' in 'LHR London Heathrow') transformed so
         * that text-to-speech synthesis will say each individual letter in the word rather than trying to pronounce
         * the letters as a single word (e.g. LAX would be pronounced as ELL-AY-EX rather than LAX).
         */
        formatUpperCaseWordsForSpeech = (text: string) : string =>
        {
            var result = '';
            text = text ? text : '';
            var chunks = text.split(' ');
            var length = chunks.length;
            for(var c = 0;c < length;c++) {
                if(c > 0) result += ' ';
                var chunk = chunks[c];
                result += VRS.stringUtility.isUpperCase(chunk) ? this.formatPunctuationForSpeech(chunk) : chunk;
            }

            return result;
        }

        /**
         * Returns a string that will cause the text-to-speech synthesis to spell out each character in the text
         * rather than pronouncing the text as a word. E.G. if passed 'HI' it will say 'aitch-eye' rather than 'hi'.
         */
        formatPunctuationForSpeech = (text: string) : string =>
        {
            var result = '';
            text = text ? text : '';
            var length = text.length;
            for(var c = 0;c < length;c++) {
                var ch = text[c];
                switch(ch) {
                    case '-':   ch = VRS.$$.SayHyphen; break;
                    case 'A':   ch = VRS.$$.SayAy; break;
                    case 'Z':   ch = VRS.$$.SayZed; break;
                }
                if(ch !== ' ') {
                    result += ch + this._PausePhrase;
                }
            }

            return result;
        }

        /**
         * Attaches event handlers to the aircraft list such that when a new aircraft is selected it will read out
         * details about the aircraft, if the configuration options at the time allow it.
         */
        annouceSelectedAircraftOnList = (aircraftList: AircraftList) =>
        {
            if(this._SelectedAircraftChangedHookResult) {
                this._AnnounceSelectedAircraftList.unhook(this._SelectedAircraftChangedHookResult);
            }
            if(this._ListUpdatedHookResult) {
                this._AnnounceSelectedAircraftList.unhook(this._ListUpdatedHookResult);
            }
            this._AnnounceSelectedAircraftList = aircraftList;
            this._SelectedAircraftChangedHookResult = aircraftList.hookSelectedAircraftChanged(this.selectedAircraftChanged, this);
            this._ListUpdatedHookResult = aircraftList.hookUpdated(this.listUpdated, this);
        }

        /**
         * Called when the currently-playing audio object has finished playing its audio.
         */
        private audioEnded = () =>
        {
            this.stopAudioTimeout();

            this._Playing.off();
            this._Playing = null;
            this.playNextEntry();
        }

        /**
         * Called after the audio object has existed for a few seconds. It's assumed that if this gets called then the
         * audio object is pointing at a bad source and should be stopped. If one source is bad then all are considered
         * bad and the queue is cleared down.
         */
        private audioTimedOut = () =>
        {
            this._AutoplayQueue = [];
            this.stopPlaying();
        }

        /**
         * Called when an aircraft list changes the selected aircraft.
         */
        private selectedAircraftChanged = () =>
        {
            if(this._AnnounceSelectedAircraftList) {
                var selectedAircraft = this._AnnounceSelectedAircraftList.getSelectedAircraft();
                if(selectedAircraft) {
                    this.announceAircraft(this._AnnounceSelectedAircraftList.getSelectedAircraft(), this._AnnounceSelectedAircraftList.getWasAircraftSelectedByUser());
                } else {
                    this._AutoplayQueue = [];
                    this.stopPlaying();
                }
            }
        }

        /**
         * Called when the aircraft list is updated.
         */
        private listUpdated = () =>
        {
            // Re-announce the aircraft, which should call out any new details.
            if(this._AnnouncedAircraft) {
                this.announceAircraft(this._AnnouncedAircraft, this._AnnouncedAircraftIsUserSelected);
            }
        }
    }
}
