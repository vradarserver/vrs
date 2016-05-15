 
/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />
/// <reference path="DatabaseEditorPluginEnums.ts" />

declare module VirtualRadar.Plugin.DatabaseEditor.WebAdmin {
    interface IViewModel {
        DataVersion: number;
        Enabled: boolean;
        Access: VirtualRadar.Interface.WebSite.WebAdminModels.IAccessModel;
        IndexPageAddress: string;
        EnumDefaultAccesses: VirtualRadar.Interface.View.IEnumModel[];
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.DatabaseEditor.WebAdmin.IViewModel;
    }
}


declare module VirtualRadar.Plugin.DatabaseEditor.WebAdmin {
    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
        Access: VirtualRadar.Interface.WebSite.WebAdminModels.IAccessModel_KO;
        IndexPageAddress: KnockoutObservable<string>;
        EnumDefaultAccesses: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.DatabaseEditor.WebAdmin.IViewModel_KO;
    }
}


