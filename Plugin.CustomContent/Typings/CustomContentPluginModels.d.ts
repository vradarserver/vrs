 
/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />
/// <reference path="CustomContentPluginEnums.ts" />

declare module VirtualRadar.Plugin.CustomContent.WebAdmin {
    interface IViewModel {
        DataVersion: number;
        Enabled: boolean;
        InjectSettings: VirtualRadar.Plugin.CustomContent.WebAdmin.IInjectSettingsModel[];
        DefaultInjectionFilesFolder: string;
        SiteRootFolder: string;
        SiteRootFolderValidation: VirtualRadar.Interface.View.IValidationModelField;
        ResourceImagesFolder: string;
        ResourceImagesFolderValidation: VirtualRadar.Interface.View.IValidationModelField;
        InjectionLocations: VirtualRadar.Interface.View.IEnumModel[];
    }
    interface IInjectSettingsModel {
        Enabled: boolean;
        PathAndFile: string;
        PathAndFileValidation: VirtualRadar.Interface.View.IValidationModelField;
        InjectionLocation: number;
        Start: boolean;
        File: string;
        FileValidation: VirtualRadar.Interface.View.IValidationModelField;
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.CustomContent.WebAdmin.IViewModel;
    }
}


declare module VirtualRadar.Plugin.CustomContent.WebAdmin {
    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
        InjectSettings: KnockoutViewModelArray<VirtualRadar.Plugin.CustomContent.WebAdmin.IInjectSettingsModel_KO>;
        DefaultInjectionFilesFolder: KnockoutObservable<string>;
        SiteRootFolder: KnockoutObservable<string>;
        SiteRootFolderValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        ResourceImagesFolder: KnockoutObservable<string>;
        ResourceImagesFolderValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        InjectionLocations: KnockoutViewModelArray<VirtualRadar.Interface.View.IEnumModel_KO>;
    }
    interface IInjectSettingsModel_KO {
        Enabled: KnockoutObservable<boolean>;
        PathAndFile: KnockoutObservable<string>;
        PathAndFileValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
        InjectionLocation: KnockoutObservable<number>;
        Start: KnockoutObservable<boolean>;
        File: KnockoutObservable<string>;
        FileValidation: VirtualRadar.Interface.View.IValidationModelField_KO;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.CustomContent.WebAdmin.IViewModel_KO;
    }
}


