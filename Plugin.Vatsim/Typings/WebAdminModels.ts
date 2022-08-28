// More info: http://frhagn.github.io/Typewriter/



/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />

declare module VirtualRadar.Plugin.Vatsim.WebAdmin {

    interface IOptionsModel {
        DataVersion: number;
        Enabled: boolean;
        RefreshIntervalSeconds: number;
        AssumeSlowAircraftAreOnGround: boolean;
        SlowAircraftThresholdSpeedKnots: number;
        InferModelFromModelType: boolean;
        ShowInvalidRegistrations: boolean;
        GeofencedFeeds: IGeofenceFeedOptionModel[];
        CentreOnTypes: IEnumModel[];
        DistanceUnitTypes: IEnumModel[];
    }

    interface IGeofenceFeedOptionModel {
        ID: string;
        FeedName: string;
        CentreOn: number;
        Latitude: number;
        Longitude: number;
        PilotCid: number;
        AirportCode: string;
        Width: number;
        Height: number;
        DistanceUnit: number;
    }

    interface ISaveOutcomeModel {
        Outcome: string;
        Options: IOptionsModel;
    }

}

declare module VirtualRadar.Plugin.Vatsim.WebAdmin {

    interface IOptionsModel_KO {
        DataVersion: KnockoutObservable<number>;
        Enabled: KnockoutObservable<boolean>;
        RefreshIntervalSeconds: KnockoutObservable<number>;
        AssumeSlowAircraftAreOnGround: KnockoutObservable<boolean>;
        SlowAircraftThresholdSpeedKnots: KnockoutObservable<number>;
        InferModelFromModelType: KnockoutObservable<boolean>;
        ShowInvalidRegistrations: KnockoutObservable<boolean>;
        GeofencedFeeds: KnockoutViewModelArray<IGeofenceFeedOptionModel_KO>;
        CentreOnTypes: KnockoutViewModelArray<IEnumModel_KO>;
        DistanceUnitTypes: KnockoutViewModelArray<IEnumModel_KO>;
    }

    interface IGeofenceFeedOptionModel_KO {
        ID: KnockoutObservable<string>;
        FeedName: KnockoutObservable<string>;
        CentreOn: KnockoutObservable<number>;
        Latitude: KnockoutObservable<number>;
        Longitude: KnockoutObservable<number>;
        PilotCid: KnockoutObservable<number>;
        AirportCode: KnockoutObservable<string>;
        Width: KnockoutObservable<number>;
        Height: KnockoutObservable<number>;
        DistanceUnit: KnockoutObservable<number>;
    }

    interface ISaveOutcomeModel_KO {
        Outcome: KnockoutObservable<string>;
        Options: IOptionsModel_KO;
    }

}