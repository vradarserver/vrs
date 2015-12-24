var TraceType = { JustSelected: 0, All: 1, None: 2 };
var DistanceUnit = { Miles: '0', Kilometres: '1', NauticalMiles: '2' };
var HeightUnit = { Feet: '0', Metres: '1' };
var SpeedUnit = { Knots: '0', MilesPerHour: '1', KilometresPerHour: '2' };

function ServerConfig()
{
    this.vrsVersion = '__VRS_VERSION';
    this.isMono = __IS_MONO;
    this.isLocalAddress = __IS_LOCAL_ADDRESS;
    this.audioEnabled = __AUDIO_ENABLED;
    this.minimumRefreshSeconds = __MINIMUM_REFRESHSECONDS;

    this.initialLatitude = __INITIAL_LATITUDE;
    this.initialLongitude = __INITIAL_LONGITUDE;
    this.initialMapType = google.maps.MapTypeId.__INITIAL_MAPTYPE;
    this.initialZoom = __INITIAL_ZOOM;
    this.initialRefreshSeconds = __INITIAL_REFRESHSECONDS;
    this.initialDistanceUnit = __INITIAL_DISTANCE_UNIT;
    this.initialHeightUnit = __INITIAL_HEIGHT_UNIT;
    this.initialSpeedUnit = __INITIAL_SPEED_UNIT;

    this.internetClientCanRunReports = __INTERNET_CLIENT_CAN_RUN_REPORTS;
    this.internetClientCanShowPinText = __INTERNET_CLIENT_CAN_SHOW_PIN_TEXT;
    this.internetClientTimeoutMinutes = __INTERNET_CLIENT_TIMEOUT_MINUTES;
    this.internetClientCanPlayAudio = __INTERNET_CLIENT_CAN_PLAY_AUDIO;
    this.internetClientCanSubmitRoutes = __INTERNET_CLIENT_CAN_SUBMIT_ROUTES;
}

var _ServerConfig = new ServerConfig();