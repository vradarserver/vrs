namespace VRS.WebAdmin
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    export enum DefaultAccess
    {
        Unrestricted = 0,
        Allow = 1,
        Deny = 2
    }

    export interface AccessModel extends ViewJson.IAccessModel_KO
    {
        CidrTableLabel?:    KnockoutComputed<string>;
        EditLabel?:         KnockoutComputed<string>;
        EditAddress?:       KnockoutObservable<string>;
        EditExisting?:      KnockoutObservable<AccessCidrModel>;
        EditIsValid?:       KnockoutComputed<boolean>;
        SaveEdit?:          () => void;
        ResetEdit?:         () => void;
        EditCidr?:          (cidrModel: AccessCidrModel) => void;
        DeleteCidr?:        (cidrModel: AccessCidrModel) => void;
    }

    export interface AccessCidrModel extends ViewJson.ICidrModel_KO
    {
        FromAddress:        KnockoutComputed<string>;
        ToAddress:          KnockoutComputed<string>;
    }

    export class AccessEditor
    {
        BuildAccessModel(model: AccessModel)
        {
            model.CidrTableLabel = ko.computed(() => {
                var result = "";
                switch(model.DefaultAccess()) {
                    case DefaultAccess.Allow:           result = VRS.Server.$$.DenyTheseAddresses; break;
                    case DefaultAccess.Deny:            result = VRS.Server.$$.AllowTheseAddresses; break;
                    case DefaultAccess.Unrestricted:    result = VRS.Server.$$.AllowTheseAddresses; break;
                }
                return result;
            });

            model.EditAddress = <KnockoutObservable<string>>ko.observable();
            model.EditExisting = <KnockoutObservable<AccessCidrModel>>ko.observable();
            model.EditLabel = ko.computed(() => {
                return !!model.EditExisting() ? VRS.Server.$$.Save : VRS.Server.$$.Add;
            });
            model.EditIsValid = ko.computed(() => {
                var address = model.EditAddress();
                var cidr = Cidr.parse(address);
                var result = !!cidr;
                var existing = model.EditExisting();
                if(result) {
                    $.each(model.Addresses(), (idx: number, other: AccessCidrModel) => {
                        if((!existing || other !== existing) && other.Cidr() === address) {
                            result = false;
                        }
                        return result;
                    });
                }
                return result;
            });
            model.SaveEdit = () => {
                if(model.EditIsValid()) {
                    var cidr = Cidr.parse(model.EditAddress());
                    var existing = model.EditExisting();
                    if(existing) {
                        existing.Cidr(cidr.toString());
                    } else {
                        model.Addresses.pushFromModel({ Cidr: cidr.toString() });
                    }
                    model.ResetEdit();
                }
            };
            model.ResetEdit = () => {
                model.EditExisting(undefined);
                model.EditAddress('');
            };

            model.EditCidr = (cidrModel: AccessCidrModel) => {
                model.EditExisting(cidrModel);
                model.EditAddress(cidrModel.Cidr());
            };
            model.DeleteCidr = (cidrModel: AccessCidrModel) => {
                model.Addresses.popToModel(cidrModel);
                if(model.EditExisting() === cidrModel) {
                    model.ResetEdit();
                }
            };
        }

        BuildAccessCidrModel(model: AccessCidrModel)
        {
            model.FromAddress = ko.computed(() => {
                var cidr = Cidr.parse(model.Cidr());
                return cidr ? cidr.getFromAddress() : '';
            });
            model.ToAddress = ko.computed(() => {
                var cidr = Cidr.parse(model.Cidr());
                return cidr ? cidr.getToAddress() : '';
            });
        }
    }

    export class Cidr
    {
        private _AddressBytes: number[];
        get AddressBytes(): number[] {
            return this._AddressBytes;
        }

        private _BitmaskBits: number;
        get BitmaskBits(): number {
            return this._BitmaskBits;
        }

        private _AddressBitmask: number;
        get AddressBitmask() : number {
            return this._AddressBitmask;
        }

        toString() : string
        {
            var result = Cidr.formatIPV4Address(this._AddressBytes);
            result += '/' + this._BitmaskBits.toString();

            return result;
        }

        equals(other: Cidr) : boolean
        {
            var result = this === other;
            if(!result && other) {
                var length = this._AddressBytes.length;
                result = length === other._AddressBytes.length && this._BitmaskBits === other._BitmaskBits;
                for(let i = 0;result && i < length;++i) {
                    if(this._AddressBytes[i] !== other._AddressBytes[i]) {
                        result = false;
                    }
                }
            }

            return result;
        }

        getFromAddress() : string
        {
            var bytes = Cidr.applyBitmask(this._AddressBytes, this._AddressBitmask, false);
            return Cidr.formatIPV4Address(bytes);
        }

        getToAddress() : string
        {
            var bytes = Cidr.applyBitmask(this._AddressBytes, this._AddressBitmask, true);
            return Cidr.formatIPV4Address(bytes);
        }

        static parse(cidr: string) : Cidr
        {
            var result: Cidr = null;

            if(cidr && cidr.length) {
                var slashIndex = cidr.indexOf('/');
                if(slashIndex === -1) {
                    slashIndex = cidr.length;
                    cidr = cidr + '/32';
                }
                var match = cidr.match( /^(\d+)\.(\d+)\.(\d+)\.(\d+)\/(\d+)$/ );
                if(match && match.length == 6) {
                    result = new Cidr();
                    result._AddressBytes = [ 0, 0, 0, 0 ];
                    result._BitmaskBits = 0;
                    for(let i = 1;i < 5;++i) {
                        var byte = Number(match[i]);
                        if(byte < 0 || byte > 255) {
                            result = null;
                            break;
                        }
                        result._AddressBytes[i - 1] = byte;
                    }
                    if(result) {
                        result._BitmaskBits = Number(match[5]);
                        if(result._BitmaskBits < 0 || result._BitmaskBits > 32) {
                            result = null;
                        }
                    }

                    if(result) {
                        var countBits = result._BitmaskBits;
                        while(countBits-- != 0) {
                            result._AddressBitmask = (result._AddressBitmask << 1) | 1;
                        }
                        result._AddressBitmask = result._AddressBitmask << (32 - result._BitmaskBits);
                    }
                }
            }

            return result;
        }

        private static applyBitmask(addressBytes: number[], addressBitmask: number, getLastMatchingAddress: boolean) : number[]
        {
            var length = addressBytes.length;
            var result: number[] = [];

            var address = 0;
            for(let i = 0;i < length;++i) {
                var byte = addressBytes[i];
                result.push(byte);
                address = (address << 8) | byte;
            }

            var bitmasked = address & addressBitmask;
            if(getLastMatchingAddress) {
                bitmasked |= ~addressBitmask;
            }

            var byteMask = 0xff;
            for(let i = length - 1;i >= 0;--i) {
                var rightShift = ((length - 1) - i) * 8;
                result[i] = (bitmasked & byteMask) >>> rightShift;
                byteMask = byteMask << 8;
            }

            return result;
        }

        private static formatIPV4Address(addressBytes: number[]) : string
        {
            var length = addressBytes.length;
            var result = '';

            for(var i = 0;i < length;++i) {
                if(result.length) {
                    result += '.';
                }
                result += String(addressBytes[i]);
            }

            return result;
        }
    }
}