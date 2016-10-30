// The declarations for our custom Modernizr tests

declare namespace __Modernizr
{
    interface FeatureDetects
    {
        autoplayaudio: boolean;         // True if the browser supports auto-play audio
        wavaudio:      boolean;         // True if the browser supports the WAV codec
    }
}