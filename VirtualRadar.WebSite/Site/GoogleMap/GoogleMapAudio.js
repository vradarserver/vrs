var GoogleMapAudioObjects = [];

function googleMapAudioOnAudioEnded(idx) { GoogleMapAudioObjects[idx].onAudioEnded(); }

function GoogleMapAudio(map, events, options)
{
    var that = this;
    var mGlobalIndex = GoogleMapAudioObjects.length;
    GoogleMapAudioObjects.push(that);

    var mMap = map;
    var mEvents = events;
    var mOptions = options;
    var mAudioQueue = [];
    var mVolumeControl;
    var mPlaying = null;

    var mEnabled = _ServerConfig.audioEnabled && _MapMode !== MapMode.flightSim;
    this.getEnabled = function() { return mEnabled; }

    var mAudioForbidden = !mEnabled || !mOptions.canPlayAudio();
    this.getForbidden = function() { return mAudioForbidden; };

    var mAudioSupported = !mAudioForbidden && testBrowserAudioSupport();
    this.getSupported = function() { return mAudioSupported; };

    var mAutoplaySupported = !mAudioForbidden && testBrowserAutoplaySupport();
    this.getAutoplaySupported = function() { return mAutoplaySupported; };

    var mAnnouncedAircraft = null;
    var mAnnouncedAircraftIsUserSelected = false;
    var mPreviouslyAnnouncedAutoSelected = [];

    var mTimeoutCounter = 0;
    var mTimeoutInterval = 10000;
    var mTimeoutLimit = 4;
    var mTimeoutLastPlaying = null;

    if(!mAudioForbidden && mAudioSupported && mAutoplaySupported) {
        setTimeout(checkTimeout, mTimeoutInterval);
        mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
        mEvents.addListener(EventId.userChangedVolume, userChangedVolumeHandler);
        mEvents.addListener(EventId.acSelected, aircraftSelectedHandler);
        mEvents.addListener(EventId.acListRefreshed, refreshHandler);

        if(typeof(GoogleMapVolumeControl) !== 'undefined') {
            mVolumeControl = new GoogleMapVolumeControl(mEvents);
            loadCookie();
            mVolumeControl.addToMap(mMap);
        }
    }

    function optionsChangedHandler(sender, args)
    {
        var stopPlayback = false;
        if(!mOptions.callOutSelected) stopPlayback = true;
        else if(mOptions.onlyCallOutAutoSelected && mAnnouncedAircraftIsUserSelected) stopPlayback = true;

        if(stopPlayback) {
            stopAllPlaying();
            mAnnouncedAircraft = null;
        }
    };

    function userChangedVolumeHandler(sender, args)
    {
        saveCookie();
        if(mPlaying !== null) {
            mPlaying.muted = mVolumeControl === undefined ? false : mVolumeControl.getMuted();
            if(!mPlaying.muted) mPlaying.volume = mVolumeControl == undefined ? 1 : mVolumeControl.getVolume() / 100;
        }
    };

    function aircraftSelectedHandler(sender, args)
    {
        if(args.aircraft !== null) announceAircraft(args.aircraft, args.userSelected);
    }

    function refreshHandler(sender, args)
    {
        if(mAnnouncedAircraft !== null) announceAircraft(mAnnouncedAircraft, mAnnouncedAircraftIsUserSelected);
    }

    function testBrowserAudioSupport()
    {
        var result = false;
        try {
            var audioElement = new Audio();
            result = audioElement.play !== undefined;
            if(result) result = audioElement.canPlayType !== undefined;
            if(result) {
                var canPlay = audioElement.canPlayType('audio/x-wav;codecs=1');
                if(canPlay === '' || canPlay === 'no') canPlay = audioElement.canPlayType('audio/wav;codecs=1'); // <- safari doesn't like x-wav
                result = canPlay !== null && canPlay !== '' && canPlay !== 'no';
            }
        } catch(e) {
            result = false;
        }

        return result;
    };

    function testBrowserAutoplaySupport()
    {
        return !isIpad() && !isIphone();
    };

    function saveCookie()
    {
        if(mVolumeControl !== undefined) {
            var nameValues = new nameValueCollection();
            nameValues.pushValue('vol', mVolumeControl.getVolume());
            nameValues.pushValue('mute', mVolumeControl.getMuted());
            writeCookie('googleMapAudio', nameValues.toString());
        }
    };

    function loadCookie()
    {
        if(mVolumeControl !== undefined) {
            var cookieValues = readCookieValues();
            var nameValues = new nameValueCollection()
            nameValues.fromString(extractCookieValue(cookieValues, 'googleMapAudio'));
            mVolumeControl.setVolume(nameValues.getValueAsNumber('vol', 75));
            mVolumeControl.setMuted(nameValues.getValueAsBool('mute', false));
        }
    }

    function playQueue()
    {
        if(mPlaying === null && mAudioQueue.length > 0) {
            var details = mAudioQueue[0];
            if(mAudioQueue.length == 1) mAudioQueue = [];
            else mAudioQueue.splice(0, 1);

            var audio = new Audio();
            audio.setAttribute('onended', 'googleMapAudioOnAudioEnded(' + mGlobalIndex + ')');
            audio.src = details.src;
            audio.volume = mVolumeControl === undefined ? 1 : mVolumeControl.getVolume() / 100;
            audio.muted = mVolumeControl === undefined ? false : mVolumeControl.getMuted();
            mPlaying = audio;
            audio.play();
        }
    };

    this.onAudioEnded = function()
    {
        mPlaying = null;
        playQueue();
    };

    function stopCurrentlyPlaying()
    {
        if(mPlaying !== null) {
            mPlaying.pause();
            mPlaying = null;
            playQueue();
        }
    }

    function stopAllPlaying()
    {
        mAudioQueue = [];
        stopCurrentlyPlaying();
    }

    function checkTimeout()
    {
        if(mPlaying === null) mTimeoutCounter = 0;
        else {
            if(mPlaying !== mTimeoutLastPlaying) mTimeoutCounter = 0;
            else {
                if(++mTimeoutCounter >= mTimeoutLimit) {
                    stopCurrentlyPlaying();
                }
            }
        }
        setTimeout(checkTimeout, mTimeoutInterval);
    }

    function say(text)
    {
        if(!mAudioForbidden && mAudioSupported && mAutoplaySupported) {
            var details = { src: 'Audio?cmd=say&line=' + encodeURIComponent(text) };
            mAudioQueue.push(details);
            playQueue();
        }
    };

    function isPreviouslyAnnouncedAutoSelected(aircraft)
    {
        var length = mPreviouslyAnnouncedAutoSelected.length;
        for(var c = 0;c < length;c++) {
            if(mPreviouslyAnnouncedAutoSelected[c] === aircraft) return true;
        }
        return false;
    }

    function recordPreviouslyAnnouncedAutoSelected(aircraft)
    {
        if(mPreviouslyAnnouncedAutoSelected.length === 5) mPreviouslyAnnouncedAutoSelected.splice(0, 1);
        mPreviouslyAnnouncedAutoSelected.push(aircraft);
    }

    function formatAcronymForSpeech(text)
    {
        var result = '';
        var length = text.length;
        for(var c = 0;c < length;c++) {
            var ch = text[c];
            switch(text[c]) {
                case 'A':   ch = 'ALpha'; break;
                case 'B':   ch = 'bravo'; break;
                case 'C':   ch = 'charlie'; break;
                case 'D':   ch = 'delta'; break;
                case 'E':   ch = 'echo'; break;
                case 'F':   ch = 'foxed-rot'; break;
                case 'G':   ch = 'golf'; break;
                case 'H':   ch = 'hotel'; break;
                case 'I':   ch = 'india'; break;
                case 'J':   ch = 'juliet'; break;
                case 'K':   ch = 'key-low'; break;
                case 'L':   ch = 'leamah'; break;
                case 'M':   ch = 'mike'; break;
                case 'N':   ch = 'november'; break;
                case 'O':   ch = 'oscar'; break;
                case 'P':   ch = 'papa'; break;
                case 'Q':   ch = 'quebec'; break;
                case 'R':   ch = 'romeo'; break;
                case 'S':   ch = 'sierra'; break;
                case 'T':   ch = 'tango'; break;
                case 'U':   ch = 'uniform'; break;
                case 'V':   ch = 'victor'; break;
                case 'W':   ch = 'whiskey'; break;
                case 'X':   ch = 'x-ray'; break;
                case 'Y':   ch = 'yankee'; break;
                case 'Z':   ch = 'zulu'; break;
                case '-':   ch = 'hyphen'; break;
            }
            if(ch !== ' ') result += ch + ' - ';
        }

        return result;
    };

    function formatUpperCaseWordsForSpeech(text)
    {
        var result = '';
        var chunks = text.split(' ');
        var length = chunks.length;
        for(var c = 0;c < length;c++) {
            if(c > 0) result += ' ';
            var chunk = chunks[c];
            result += isUpperCaseWord(chunk) ? formatPunctuationForSpeech(chunk) : chunk;
        }

        return result;
    };
    
    function formatPunctuationForSpeech(text)
    {
        var result = '';
        var length = text.length;
        for(var c = 0;c < length;c++) {
            var ch = text[c];
            if(ch === '-') ch = 'hyphen';
            if(ch === 'Z') ch = 'ZED';
            if(ch !== ' ') result += ch + ' - ';
        }

        return result;
    };

    function isUpperCaseWord(text)
    {
        var result = text.length > 0;
        if(result) result = text.toUpperCase() == text;

        return result;
    }

    function announceAircraft(aircraft, userSelected)
    {
        var newAircraft = mAnnouncedAircraft === null || mAnnouncedAircraft !== aircraft;
        var ignore = mAudioForbidden || !mAudioSupported || !mAutoplaySupported;
        if(!ignore) ignore = !mOptions.callOutSelected || (mOptions.onlyCallOutAutoSelected && userSelected);
        if(!ignore) ignore = newAircraft && !userSelected && isPreviouslyAnnouncedAutoSelected(aircraft); // stop auto-select 'ping-pong' effect
 
        if(!ignore) {
            if(newAircraft && !userSelected) recordPreviouslyAnnouncedAutoSelected(aircraft);

            var sayText = '';
            var appendRoute = false;

            if(aircraft === mAnnouncedAircraft) {
                if(aircraft.Reg.length > 0 && aircraft.RegChanged) sayText += 'Registration ' + formatAcronymForSpeech(aircraft.Reg) + '. ';
                if(aircraft.Call.length > 0 && aircraft.CallChanged) {
                    sayText += 'Call sign ' + formatAcronymForSpeech(aircraft.Call) + '. ';
                    appendRoute = true;
                }
            } else if(newAircraft) {
                if(aircraft.Reg !== '') sayText += 'Registration ' + formatAcronymForSpeech(aircraft.Reg) + '. ';
                else sayText += 'eye co ' + formatAcronymForSpeech(aircraft.Icao) + '. ';
                if(aircraft.Type !== '') sayText += 'Type ' + formatUpperCaseWordsForSpeech(aircraft.Type) + '. '
                if(aircraft.Op !== '') sayText += 'Operated by ' + formatUpperCaseWordsForSpeech(aircraft.Op) + '. ';
                if(aircraft.Call !== '') {
                    sayText += 'Call sign ' + formatAcronymForSpeech(aircraft.Call) + '. ';
                    appendRoute = true;
                }
            }

            if(appendRoute) {
                if(aircraft.From === '') sayText += 'Route not known.';
                else {
                    sayText += 'Travelling from ' + formatUpperCaseWordsForSpeech(aircraft.From);
                    if(aircraft.Stops.length > 0) {
                        sayText += ' via ';
                        for(var c = 0;c < aircraft.Stops.length;++c) {
                            sayText += formatUpperCaseWordsForSpeech(aircraft.Stops[c]);
                            sayText += ', ';
                            if(c + 2 == aircraft.Stops.length) sayText += 'and ';
                        }
                    }
                    if(aircraft.To !== '') sayText += ' to ' + formatUpperCaseWordsForSpeech(aircraft.To);
                    sayText += '. ';
                }
            }

            mAnnouncedAircraft = aircraft;
            mAnnouncedAircraftIsUserSelected = userSelected;
            if(newAircraft) stopAllPlaying();
            if(sayText.length > 0) say(sayText);
        }
    };
}