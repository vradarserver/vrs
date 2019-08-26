using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.TileServerCache
{
    class TileServerSettingsManagerWrapper : ITileServerSettingsManager
    {
        /// <summary>
        /// The actual tile server settings manager that we're wrapping.
        /// </summary>
        private static ITileServerSettingsManager _DefaultImplementation;

        /// <summary>
        /// The object that will translate between real and fake tile server URLs for us.
        /// </summary>
        private static TileServerUrlTranslator _TileServerUrlTranslator;

        /// <summary>
        /// See interface. Retained for backwards compatability.
        /// </summary>
        public ITileServerSettingsManager Singleton => Factory.ResolveSingleton<ITileServerSettingsManager>();

        /// <summary>
        /// See interface.
        /// </summary>
        public DateTime LastDownloadUtc { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler TileServerSettingsDownloaded;

        /// <summary>
        /// Raises <see cref="TileServerSettingsDownloaded"/>.
        /// </summary>
        /// <param name="args"></param>
        internal void RaiseTileServerSettingsDownloaded(EventArgs args)
        {
            LastDownloadUtc = DateTime.UtcNow;
            EventHelper.Raise<EventArgs>(TileServerSettingsDownloaded, this, args);
        }

        /// <summary>
        /// Initialises the wrapper.
        /// </summary>
        /// <param name="classFactory"></param>
        /// <returns></returns>
        public static TileServerSettingsManagerWrapper Initialise(IClassFactory classFactory)
        {
            _DefaultImplementation = classFactory.ResolveSingleton<ITileServerSettingsManager>();

            var singleton = new TileServerSettingsManagerWrapper();
            classFactory.RegisterInstance<ITileServerSettingsManager>(singleton);

            _TileServerUrlTranslator = new TileServerUrlTranslator();

            singleton.HookEvents();

            return singleton;
        }

        private void HookEvents()
        {
            _DefaultImplementation.TileServerSettingsDownloaded += DefaultImplementation_TileServerSettingsDownloaded;
        }

        private void DefaultImplementation_TileServerSettingsDownloaded(object sender, EventArgs e)
        {
            RaiseTileServerSettingsDownloaded(e);
        }

        /// <summary>
        /// See interface.
        /// </summary>
        public void DownloadTileServerSettings()
        {
            _DefaultImplementation.DownloadTileServerSettings();
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public TileServerSettings[] GetAllTileLayerSettings(MapProvider mapProvider)
        {
            var realResult = _DefaultImplementation.GetAllTileLayerSettings(mapProvider);
            return ServeRealUrls(mapProvider)
                ? realResult
                : CloneAndReplaceManyRealTileServerSettingsWithFake(mapProvider, realResult);
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public TileServerSettings[] GetAllTileServerSettings(MapProvider mapProvider)
        {
            var realResult = _DefaultImplementation.GetAllTileServerSettings(mapProvider);
            return ServeRealUrls(mapProvider)
                ? realResult
                : CloneAndReplaceManyRealTileServerSettingsWithFake(mapProvider, realResult);
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public TileServerSettings GetDefaultTileServerSettings(MapProvider mapProvider)
        {
            var realResult = _DefaultImplementation.GetDefaultTileServerSettings(mapProvider);
            return ServeRealUrls(mapProvider)
                ? realResult
                : CloneAndReplaceRealTileServerSettingsWithFake(mapProvider, realResult);
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="fallbackToDefaultIfMissing"></param>
        /// <returns></returns>
        public TileServerSettings GetTileServerSettings(MapProvider mapProvider, string name, bool fallbackToDefaultIfMissing)
        {
            var realResult = _DefaultImplementation.GetTileServerSettings(mapProvider, name, fallbackToDefaultIfMissing);
            return ServeRealUrls(mapProvider)
                ? realResult
                : CloneAndReplaceRealTileServerSettingsWithFake(mapProvider, realResult);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <param name="includeTileServers"></param>
        /// <param name="includeTileLayers"></param>
        /// <returns></returns>
        public TileServerSettings GetTileServerOrLayerSettings(MapProvider mapProvider, string name, bool includeTileServers, bool includeTileLayers)
        {
            var realResult = _DefaultImplementation.GetTileServerOrLayerSettings(mapProvider, name, includeTileServers, includeTileLayers);
            return ServeRealUrls(mapProvider)
                ? realResult
                : CloneAndReplaceRealTileServerSettingsWithFake(mapProvider, realResult);
        }

        /// <summary>
        /// See interface.
        /// </summary>
        public void Initialise()
        {
            _DefaultImplementation.Initialise();
        }

        /// <summary>
        /// See interface.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        public bool MapProviderUsesTileServers(MapProvider mapProvider)
        {
            return _DefaultImplementation.MapProviderUsesTileServers(mapProvider);
        }

        /// <summary>
        /// Returns the actual tile server settings for the map provider and name passed across.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public TileServerSettings GetRealTileServerSettings(MapProvider mapProvider, string name)
        {
            return _DefaultImplementation.GetTileServerOrLayerSettings(
                mapProvider,
                name,
                includeTileServers: true,
                includeTileLayers: true
            );
        }

        /// <summary>
        /// Returns true if real URLs should be offered up.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <returns></returns>
        private bool ServeRealUrls(MapProvider mapProvider)
        {
            var options = Plugin.Singleton?.Options;
            return mapProvider != MapProvider.Leaflet || !(options?.IsPluginEnabled ?? false);
        }

        /// <summary>
        /// Returns a clone of the settings passed in with the real URL swapped out with the fake one.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private TileServerSettings CloneAndReplaceRealTileServerSettingsWithFake(MapProvider mapProvider, TileServerSettings settings)
        {
            var result = settings;

            var options = Plugin.Singleton?.Options;
            var canCacheMap = options?.CacheMapTiles ?? false;
            var canCacheLayer = options?.CacheLayerTiles ?? false;
            var canCacheThis = (canCacheMap && !settings.IsLayer) || (canCacheLayer && settings.IsLayer);

            if(settings != null && canCacheThis) {
                result = (TileServerSettings)settings.Clone();
                result.Url = _TileServerUrlTranslator.ToFakeUrl(mapProvider, result);
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of tile server settings that are clones of the ones passed in but with the real
        /// URLs swapped out for fake ones.
        /// </summary>
        /// <param name="mapProvider"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private TileServerSettings[] CloneAndReplaceManyRealTileServerSettingsWithFake(MapProvider mapProvider, IEnumerable<TileServerSettings> settings)
        {
            return settings
                .Select(r => CloneAndReplaceRealTileServerSettingsWithFake(mapProvider, r))
                .ToArray();
        }
    }
}
