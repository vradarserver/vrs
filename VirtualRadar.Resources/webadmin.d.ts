declare module System.Net.Sockets {
    const enum AddressFamily {
        Unknown = -1,
        Unspecified = 0,
        Unix = 1,
        InterNetwork = 2,
        ImpLink = 3,
        Pup = 4,
        Chaos = 5,
        NS = 6,
        Ipx = 6,
        Iso = 7,
        Osi = 7,
        Ecma = 8,
        DataKit = 9,
        Ccitt = 10,
        Sna = 11,
        DecNet = 12,
        DataLink = 13,
        Lat = 14,
        HyperChannel = 15,
        AppleTalk = 16,
        NetBios = 17,
        VoiceView = 18,
        FireFox = 19,
        Banyan = 21,
        Atm = 22,
        InterNetworkV6 = 23,
        Cluster = 24,
        Ieee12844 = 25,
        Irda = 26,
        NetworkDesigners = 28,
        Max = 29,
    }
}
declare module VirtualRadar.Interface.Network {
    const enum ConnectionStatus {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        CannotConnect = 3,
        Reconnecting = 4,
        Waiting = 5,
    }
}
declare module VirtualRadar.Interface.Settings {
    const enum ReceiverUsage {
        Normal = 0,
        HideFromWebSite = 1,
        MergeOnly = 2,
    }
}
declare namespace Bootstrap {
    class Helper {
        static decorateBootstrapElements(): void;
        static decorateValidationElements(): void;
        static decorateValidationFieldValidate(): void;
        static decorateValidationIcons(): void;
        static decorateCollapsiblePanels(): void;
        static decorateModals(): void;
        private static getOptions(element);
        private static getFieldName(element);
        private static _UniqueId;
        private static applyUniqueId(element);
    }
}
declare namespace VRS.WebAdmin {
    class Menu {
        private _MenuEntries;
        private _MenuItemsList;
        constructor();
        private fetchMenuEntries();
        private addTopNavbar();
        private addNavSidebar();
        private populateMenu();
    }
    var menu: Menu;
}
declare namespace VRS.WebAdmin {
    class ViewId {
        private _LostContact;
        private _FailedAttempts;
        private _ModalOverlay;
        private _ShowModalOverlayTimer;
        private _Id;
        Id: string;
        private _ViewName;
        ViewName: string;
        constructor(viewName: string, viewId?: string);
        private setHeartbeatTimer(pauseInterval?);
        private sendHeartbeat();
        showModalOverlay(show: boolean): void;
        isModalOverlayVisible(): boolean;
        ajax(methodName: string, settings?: JQueryAjaxSettings, showModalOverlay?: boolean, keepOverlayWhenFinished?: boolean): JQueryXHR;
        private buildMethodUrl(methodName);
        private addViewIdToSettings(settings);
        private isDeferredExecutionResponse(response);
        private fetchDeferredExecutionResponse(jobId, success, interval, removeOverlay);
        private sendRequestForDeferredExecutionResponse(jobId, success, removeOverlay);
        createWrapupValidation(validationFields: VirtualRadar.Interface.View.IValidationModelField_KO[]): IValidation_KC;
        createArrayWrapupValidation<T>(array: KnockoutObservableArray<T>, getWrapUp: (item: T) => IValidation_KC): IValidation_KC;
        findValidationProperties(model: Object, appendToArray?: VirtualRadar.Interface.View.IValidationModelField_KO[]): VirtualRadar.Interface.View.IValidationModelField_KO[];
        describeEnum(enumValue: number, enumModels: VirtualRadar.Interface.View.IEnumModel[]): string;
    }
}
