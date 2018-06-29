 
/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />
/// <reference path="FeedFilterPluginEnums.ts" />

declare module VirtualRadar.Plugin.FeedFilter.WebAdmin {
    interface IViewModel {
        DataVersion: number;
        Enabled: boolean;
        ProhibitUnfilterableFeeds: boolean;
        Access: VirtualRadar.Interface.WebSite.WebAdminModels.IAccessModel;
        EnumDefaultAccesses: VirtualRadar.Interface.View.IEnumModel[];
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.FeedFilter.WebAdmin.IViewModel;
    }
}


declare module VirtualRadar.Plugin.FeedFilter.WebAdmin {
    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
        ProhibitUnfilterableFeeds: KnockoutObservable<boolean>;
        Access: VirtualRadar.Interface.WebSite.WebAdminModels.IAccessModel_KO;
        EnumDefaultAccesses: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.FeedFilter.WebAdmin.IViewModel_KO;
    }
}


