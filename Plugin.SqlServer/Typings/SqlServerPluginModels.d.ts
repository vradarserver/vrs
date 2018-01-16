 
/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />
/// <reference path="SqlServerPluginEnums.ts" />

declare module VirtualRadar.Plugin.SqlServer.WebAdmin {
    interface IViewModel {
        DataVersion: number;
        Enabled: boolean;
        ConnectionString: string;
        CommandTimeoutSeconds: number;
    }
    interface ISaveOutcomeModel {
        Outcome: string;
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel;
    }
    interface ITestConnectionOutcomeModel {
        Title: string;
        Message: string;
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel;
    }
    interface IUpdateSchemaOutcomeModel {
        Title: string;
        OutputLines: string[];
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel;
    }
}


declare module VirtualRadar.Plugin.SqlServer.WebAdmin {
    interface IViewModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
        ConnectionString: KnockoutObservable<string>;
        CommandTimeoutSeconds: KnockoutObservable<number>;
    }
    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel_KO;
    }
    interface ITestConnectionOutcomeModel_KO {
        Title: KnockoutObservable<string>;
        Message: KnockoutObservable<string>;
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel_KO;
    }
    interface IUpdateSchemaOutcomeModel_KO {
        Title: KnockoutObservable<string>;
        OutputLines: KnockoutViewModelArray<string>;
        ViewModel: VirtualRadar.Plugin.SqlServer.WebAdmin.IViewModel_KO;
    }
}


