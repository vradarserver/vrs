 
/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />
/// <reference path="DatabaseWriterPluginEnums.ts" />

declare module VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin {
    interface IViewModel {
        PluginEnabled: boolean;
        AllowUpdateOfOtherDatabases: boolean;
        DatabaseFileName: string;
        ReceiverId: number;
        SaveDownloadedAircraftDetails: boolean;
        RefreshOutOfDateAircraft: boolean;
        CombinedFeeds: VirtualRadar.Plugin.BaseStationDatabaseWriter.ICombinedFeed[];
        OnlineLookupWriteActionNotice: string;
    }
    interface ICreateDatabaseOutcomeModel {
        Title: string;
        Message: string;
        ViewModel: VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin.IViewModel;
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin.IViewModel;
    }
}
declare module VirtualRadar.Plugin.BaseStationDatabaseWriter {
    interface ICombinedFeed {
        UniqueId: number;
        Name: string;
    }
}


declare module VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin {
    interface IViewModel_KO {
        PluginEnabled: KnockoutObservable<boolean>;
        AllowUpdateOfOtherDatabases: KnockoutObservable<boolean>;
        DatabaseFileName: KnockoutObservable<string>;
        ReceiverId: KnockoutObservable<number>;
        SaveDownloadedAircraftDetails: KnockoutObservable<boolean>;
        RefreshOutOfDateAircraft: KnockoutObservable<boolean>;
        CombinedFeeds: KnockoutViewModelArray<VirtualRadar.Plugin.BaseStationDatabaseWriter.ICombinedFeed_KO>;
        OnlineLookupWriteActionNotice: KnockoutObservable<string>;
    }
    interface ICreateDatabaseOutcomeModel_KO {
        Title: KnockoutObservable<string>;
        Message: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin.IViewModel_KO;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin.IViewModel_KO;
    }
}
declare module VirtualRadar.Plugin.BaseStationDatabaseWriter {
    interface ICombinedFeed_KO {
        UniqueId: KnockoutObservable<number>;
        Name: KnockoutObservable<string>;
    }
}


