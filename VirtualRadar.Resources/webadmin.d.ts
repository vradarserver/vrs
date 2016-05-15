declare namespace VRS.WebAdmin {
    enum DefaultAccess {
        Unrestricted = 0,
        Allow = 1,
        Deny = 2,
    }
    interface AccessModel extends VirtualRadar.Interface.WebSite.WebAdminModels.IAccessModel_KO {
        CidrTableLabel?: KnockoutComputed<string>;
        EditLabel?: KnockoutComputed<string>;
        EditAddress?: KnockoutObservable<string>;
        EditExisting?: KnockoutObservable<AccessCidrModel>;
        EditIsValid?: KnockoutComputed<boolean>;
        SaveEdit?: () => void;
        ResetEdit?: () => void;
        EditCidr?: (cidrModel: AccessCidrModel) => void;
        DeleteCidr?: (cidrModel: AccessCidrModel) => void;
    }
    interface AccessCidrModel extends VirtualRadar.Interface.WebSite.WebAdminModels.ICidrModel_KO {
        FromAddress: KnockoutComputed<string>;
        ToAddress: KnockoutComputed<string>;
    }
    class AccessEditor {
        BuildAccessModel(model: AccessModel): void;
        BuildAccessCidrModel(model: AccessCidrModel): void;
    }
    class Cidr {
        private _AddressBytes;
        AddressBytes: number[];
        private _BitmaskBits;
        BitmaskBits: number;
        private _AddressBitmask;
        AddressBitmask: number;
        toString(): string;
        equals(other: Cidr): boolean;
        getFromAddress(): string;
        getToAddress(): string;
        static parse(cidr: string): Cidr;
        private static applyBitmask(addressBytes, addressBitmask, getLastMatchingAddress);
        private static formatIPV4Address(addressBytes);
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
        createArrayWrapupValidation<T>(array: KnockoutObservableArray<T>, getWrapUp: (item: T) => IValidation_KC, ...includeValidations: IValidation_KC[]): IValidation_KC;
        findValidationProperties(model: Object, filter?: (name: string, value: VirtualRadar.Interface.View.IValidationModelField_KO) => boolean, appendToArray?: VirtualRadar.Interface.View.IValidationModelField_KO[]): VirtualRadar.Interface.View.IValidationModelField_KO[];
        describeEnum(enumValue: number, enumModels: VirtualRadar.Interface.View.IEnumModel[]): string;
    }
}
