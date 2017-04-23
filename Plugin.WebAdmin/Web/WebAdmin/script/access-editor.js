var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        (function (DefaultAccess) {
            DefaultAccess[DefaultAccess["Unrestricted"] = 0] = "Unrestricted";
            DefaultAccess[DefaultAccess["Allow"] = 1] = "Allow";
            DefaultAccess[DefaultAccess["Deny"] = 2] = "Deny";
        })(WebAdmin.DefaultAccess || (WebAdmin.DefaultAccess = {}));
        var DefaultAccess = WebAdmin.DefaultAccess;
        var AccessEditor = (function () {
            function AccessEditor() {
            }
            AccessEditor.prototype.BuildAccessModel = function (model) {
                model.CidrTableLabel = ko.computed(function () {
                    var result = "";
                    switch (model.DefaultAccess()) {
                        case DefaultAccess.Allow:
                            result = VRS.Server.$$.DenyTheseAddresses;
                            break;
                        case DefaultAccess.Deny:
                            result = VRS.Server.$$.AllowTheseAddresses;
                            break;
                        case DefaultAccess.Unrestricted:
                            result = VRS.Server.$$.AllowTheseAddresses;
                            break;
                    }
                    return result;
                });
                model.EditAddress = ko.observable();
                model.EditExisting = ko.observable();
                model.EditLabel = ko.computed(function () {
                    return !!model.EditExisting() ? VRS.Server.$$.Save : VRS.Server.$$.Add;
                });
                model.EditIsValid = ko.computed(function () {
                    var address = model.EditAddress();
                    var cidr = Cidr.parse(address);
                    var result = !!cidr;
                    var existing = model.EditExisting();
                    if (result) {
                        $.each(model.Addresses(), function (idx, other) {
                            if (!existing || other !== existing) {
                                var otherCidr = Cidr.parse(other.Cidr());
                                result = !cidr.equals(otherCidr);
                            }
                            return result;
                        });
                    }
                    return result;
                });
                model.SaveEdit = function () {
                    if (model.EditIsValid()) {
                        var cidr = Cidr.parse(model.EditAddress());
                        var existing = model.EditExisting();
                        if (existing) {
                            existing.Cidr(cidr.toString());
                        }
                        else {
                            model.Addresses.pushFromModel({ Cidr: cidr.toString() });
                        }
                        model.ResetEdit();
                    }
                };
                model.ResetEdit = function () {
                    model.EditExisting(undefined);
                    model.EditAddress('');
                };
                model.EditCidr = function (cidrModel) {
                    model.EditExisting(cidrModel);
                    model.EditAddress(cidrModel.Cidr());
                };
                model.DeleteCidr = function (cidrModel) {
                    var idx = VRS.arrayHelper.indexOf(model.Addresses(), cidrModel);
                    if (idx !== -1) {
                        model.Addresses.removeAtToModel(idx, cidrModel);
                        if (model.EditExisting() === cidrModel) {
                            model.ResetEdit();
                        }
                    }
                };
            };
            AccessEditor.prototype.BuildAccessCidrModel = function (model) {
                model.FromAddress = ko.computed(function () {
                    var cidr = Cidr.parse(model.Cidr());
                    return cidr ? cidr.getFromAddress() : '';
                });
                model.ToAddress = ko.computed(function () {
                    var cidr = Cidr.parse(model.Cidr());
                    return cidr ? cidr.getToAddress() : '';
                });
            };
            return AccessEditor;
        }());
        WebAdmin.AccessEditor = AccessEditor;
        var Cidr = (function () {
            function Cidr() {
            }
            Object.defineProperty(Cidr.prototype, "AddressBytes", {
                get: function () {
                    return this._AddressBytes;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(Cidr.prototype, "BitmaskBits", {
                get: function () {
                    return this._BitmaskBits;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(Cidr.prototype, "AddressBitmask", {
                get: function () {
                    return this._AddressBitmask;
                },
                enumerable: true,
                configurable: true
            });
            Cidr.prototype.toString = function () {
                var result = Cidr.formatIPV4Address(this._AddressBytes);
                result += '/' + this._BitmaskBits.toString();
                return result;
            };
            Cidr.prototype.equals = function (other) {
                var result = this === other;
                if (!result && other) {
                    result = this.getFromAddress() === other.getFromAddress() &&
                        this.getToAddress() === other.getToAddress();
                }
                return result;
            };
            Cidr.prototype.getFromAddress = function () {
                var bytes = Cidr.applyBitmask(this._AddressBytes, this._AddressBitmask, false);
                return Cidr.formatIPV4Address(bytes);
            };
            Cidr.prototype.getToAddress = function () {
                var bytes = Cidr.applyBitmask(this._AddressBytes, this._AddressBitmask, true);
                return Cidr.formatIPV4Address(bytes);
            };
            Cidr.parse = function (cidr) {
                var result = null;
                if (cidr && cidr.length) {
                    var slashIndex = cidr.indexOf('/');
                    if (slashIndex === -1) {
                        slashIndex = cidr.length;
                        cidr = cidr + '/32';
                    }
                    var match = cidr.match(/^(\d+)\.(\d+)\.(\d+)\.(\d+)\/(\d+)$/);
                    if (match && match.length == 6) {
                        result = new Cidr();
                        result._AddressBytes = [0, 0, 0, 0];
                        result._BitmaskBits = 0;
                        for (var i = 1; i < 5; ++i) {
                            var byte = Number(match[i]);
                            if (byte < 0 || byte > 255) {
                                result = null;
                                break;
                            }
                            result._AddressBytes[i - 1] = byte;
                        }
                        if (result) {
                            result._BitmaskBits = Number(match[5]);
                            if (result._BitmaskBits < 0 || result._BitmaskBits > 32) {
                                result = null;
                            }
                        }
                        if (result) {
                            var countBits = result._BitmaskBits;
                            while (countBits-- != 0) {
                                result._AddressBitmask = (result._AddressBitmask << 1) | 1;
                            }
                            result._AddressBitmask = result._AddressBitmask << (32 - result._BitmaskBits);
                        }
                    }
                }
                return result;
            };
            Cidr.applyBitmask = function (addressBytes, addressBitmask, getLastMatchingAddress) {
                var length = addressBytes.length;
                var result = [];
                var address = 0;
                for (var i = 0; i < length; ++i) {
                    var byte = addressBytes[i];
                    result.push(byte);
                    address = (address << 8) | byte;
                }
                var bitmasked = address & addressBitmask;
                if (getLastMatchingAddress) {
                    bitmasked |= ~addressBitmask;
                }
                var byteMask = 0xff;
                for (var i = length - 1; i >= 0; --i) {
                    var rightShift = ((length - 1) - i) * 8;
                    result[i] = (bitmasked & byteMask) >>> rightShift;
                    byteMask = byteMask << 8;
                }
                return result;
            };
            Cidr.formatIPV4Address = function (addressBytes) {
                var length = addressBytes.length;
                var result = '';
                for (var i = 0; i < length; ++i) {
                    if (result.length) {
                        result += '.';
                    }
                    result += String(addressBytes[i]);
                }
                return result;
            };
            return Cidr;
        }());
        WebAdmin.Cidr = Cidr;
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=access-editor.js.map