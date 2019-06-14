// More info: http://frhagn.github.io/Typewriter/



/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />

declare module VirtualRadar.Plugin.TileServerCache.WebAdmin {

    interface IViewModel {
        DataVersion: number;
        IsPluginEnabled: boolean;
        IsOfflineModeEnabled: boolean;
        UseDefaultCacheFolder: boolean;
        CacheFolderOverride: string;
        CacheFolderOverrideValidation: VirtualRadar.Interface.View.IValidationModelField;
        TileServerTimeoutSeconds: number;
        CacheMapTiles: boolean;
        CacheLayerTiles: boolean;
    }

    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: IViewModel;
    }

}

declare module VirtualRadar.Plugin.TileServerCache.WebAdmin {

    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        IsPluginEnabled: KnockoutObservable<boolean>;
        IsOfflineModeEnabled: KnockoutObservable<boolean>;
        UseDefaultCacheFolder: KnockoutObservable<boolean>;
        CacheFolderOverride: KnockoutObservable<string>;
        CacheFolderOverrideValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        TileServerTimeoutSeconds: KnockoutObservable<number>;
        CacheMapTiles: KnockoutObservable<boolean>;
        CacheLayerTiles: KnockoutObservable<boolean>;
    }

    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: IViewModel_KO;
    }

}