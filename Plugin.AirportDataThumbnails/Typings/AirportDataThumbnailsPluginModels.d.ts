/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />

declare module VirtualRadar.Plugin.AirportDataThumbnails.WebAdmin {
    interface IViewModel {
        DataVersion: number;
        Enabled: boolean;
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.AirportDataThumbnails.WebAdmin.IViewModel;
    }
}

declare module VirtualRadar.Plugin.AirportDataThumbnails.WebAdmin {
    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.AirportDataThumbnails.WebAdmin.IViewModel_KO;
    }
}
