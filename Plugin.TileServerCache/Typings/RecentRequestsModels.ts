// More info: http://frhagn.github.io/Typewriter/



/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />

declare module VirtualRadar.Plugin.TileServerCache.WebAdmin {

    interface IRecentRequestModel {
        ID: number;
        ReceivedUtc: string;
        DurationMs: string;
        TileServerName: string;
        Zoom: string;
        X: string;
        Y: string;
        Retina: string;
        Outcome: string;
    }

    interface IRecentRequestsViewModel {
        RecentRequests: IRecentRequestModel[];
    }

}

declare module VirtualRadar.Plugin.TileServerCache.WebAdmin {

    interface IRecentRequestModel_KO {
        ID: KnockoutObservable<number>;
        ReceivedUtc: KnockoutObservable<string>;
        DurationMs: KnockoutObservable<string>;
        TileServerName: KnockoutObservable<string>;
        Zoom: KnockoutObservable<string>;
        X: KnockoutObservable<string>;
        Y: KnockoutObservable<string>;
        Retina: KnockoutObservable<string>;
        Outcome: KnockoutObservable<string>;
    }

    interface IRecentRequestsViewModel_KO {
        RecentRequests: KnockoutViewModelArray<IRecentRequestModel_KO>;
    }

}