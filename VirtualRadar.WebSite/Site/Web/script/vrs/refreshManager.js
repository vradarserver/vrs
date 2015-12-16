var VRS;
(function (VRS) {
    var RefreshTarget = (function () {
        function RefreshTarget(settings) {
            this.callOnRefresh = function () {
                this.onRefresh.call(this.onRefreshThis);
            };
            this._Settings = settings;
        }
        Object.defineProperty(RefreshTarget.prototype, "targetJQ", {
            get: function () {
                return this._Settings.targetJQ;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(RefreshTarget.prototype, "onRefresh", {
            get: function () {
                return this._Settings.onRefresh;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(RefreshTarget.prototype, "onRefreshThis", {
            get: function () {
                return this._Settings.onRefreshThis || window;
            },
            enumerable: true,
            configurable: true
        });
        RefreshTarget.prototype.dispose = function () {
            this._Settings = null;
            this.targetJQ = null;
            this.onRefresh = null;
            this.onRefreshThis = null;
        };
        return RefreshTarget;
    })();
    VRS.RefreshTarget = RefreshTarget;
    var RefreshOwner = (function () {
        function RefreshOwner(settings) {
            this._Settings = settings;
            this._Targets = settings.targets || [];
        }
        Object.defineProperty(RefreshOwner.prototype, "ownerJQ", {
            get: function () {
                return this._Settings.ownerJQ;
            },
            enumerable: true,
            configurable: true
        });
        RefreshOwner.prototype.dispose = function () {
            this._Settings = null;
            this.ownerJQ = null;
            this._Targets = [];
        };
        RefreshOwner.prototype.refreshTargets = function () {
            var length = this._Targets.length;
            for (var i = 0; i < length; ++i) {
                this._Targets[i].callOnRefresh();
            }
        };
        RefreshOwner.prototype.getTarget = function (elementJQ) {
            var index = this.getTargetIndex(elementJQ);
            return index === -1 ? null : this._Targets[index];
        };
        ;
        RefreshOwner.prototype.getTargetIndex = function (elementJQ) {
            var result = -1;
            var length = this._Targets.length;
            for (var i = 0; i < length; ++i) {
                var target = this._Targets[i];
                if (target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        RefreshOwner.prototype.addTarget = function (target) {
            if (this.getTargetIndex(target.targetJQ) === -1) {
                this._Targets.push(target);
            }
        };
        RefreshOwner.prototype.removeTarget = function (target) {
            var index = this.getTargetIndex(target.targetJQ);
            if (index !== -1)
                this._Targets.splice(index, 1);
        };
        RefreshOwner.prototype.removeAllTargets = function () {
            this._Targets = [];
        };
        return RefreshOwner;
    })();
    VRS.RefreshOwner = RefreshOwner;
    var RefreshManager = (function () {
        function RefreshManager() {
            this._Targets = [];
            this._Owners = [];
        }
        RefreshManager.prototype.registerTarget = function (elementJQ, onRefresh, onRefreshThis) {
            if (this.getTargetIndex(elementJQ) === -1) {
                var target = new VRS.RefreshTarget({
                    targetJQ: elementJQ,
                    onRefresh: onRefresh,
                    onRefreshThis: onRefreshThis
                });
                this._Targets.push(target);
                var allOwners = this.buildOwners(elementJQ);
                var length = allOwners.length;
                for (var i = 0; i < length; ++i) {
                    var owner = allOwners[i];
                    owner.addTarget(target);
                }
            }
        };
        RefreshManager.prototype.unregisterTarget = function (elementJQ) {
            var index = this.getTargetIndex(elementJQ);
            if (index !== -1) {
                var target = this._Targets[index];
                var length = this._Owners.length;
                for (var i = 0; i < length; ++i) {
                    this._Owners[i].removeTarget(target);
                }
                this._Targets.splice(index, 1);
                target.dispose();
            }
        };
        RefreshManager.prototype.registerOwner = function (elementJQ) {
            if (this.getOwnerIndex(elementJQ) === -1) {
                var targets = [];
                var targetLength = this._Targets.length;
                for (var i = 0; i < targetLength; ++i) {
                    var target = this._Targets[i];
                    target.targetJQ.parents().each(function (idx, parentElement) {
                        var continueEach = true;
                        if (elementJQ.is(parentElement)) {
                            targets.push(target);
                            continueEach = false;
                        }
                        return continueEach;
                    });
                }
                var owner = new VRS.RefreshOwner({
                    ownerJQ: elementJQ,
                    targets: targets
                });
                this._Owners.push(owner);
            }
        };
        RefreshManager.prototype.unregisterOwner = function (elementJQ) {
            var index = this.getOwnerIndex(elementJQ);
            if (index !== -1) {
                var owner = this._Owners[index];
                owner.dispose();
                this._Owners.splice(index, 1);
            }
        };
        RefreshManager.prototype.rebuildRelationships = function () {
            var length;
            var i;
            length = this._Owners.length;
            for (i = 0; i < length; ++i) {
                this._Owners[i].removeAllTargets();
            }
            length = this._Targets.length;
            for (i = 0; i < length; ++i) {
                var target = this._Targets[i];
                var owners = this.buildOwners(target.targetJQ);
                var ownersLength = owners.length;
                for (var j = 0; j < ownersLength; ++j) {
                    owners[j].addTarget(target);
                }
            }
        };
        RefreshManager.prototype.refreshTargets = function (ownerJQ) {
            var index = this.getOwnerIndex(ownerJQ);
            if (index !== -1)
                this._Owners[index].refreshTargets();
        };
        RefreshManager.prototype.getTargetIndex = function (elementJQ) {
            var result = -1;
            var length = this._Targets.length;
            for (var i = 0; i < length; ++i) {
                var target = this._Targets[i];
                if (target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        RefreshManager.prototype.getOwnerIndex = function (elementJQ) {
            var result = -1;
            var length = this._Owners.length;
            for (var i = 0; i < length; ++i) {
                var owner = this._Owners[i];
                if (owner.ownerJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        RefreshManager.prototype.buildOwners = function (elementJQ) {
            var result = [];
            var parents = elementJQ.parents();
            var ownerLength = this._Owners.length;
            for (var i = 0; i < ownerLength; ++i) {
                var owner = this._Owners[i];
                parents.each(function (idx, parentElement) {
                    if (owner.ownerJQ.is(parentElement))
                        result.push(owner);
                });
            }
            return result;
        };
        return RefreshManager;
    })();
    VRS.RefreshManager = RefreshManager;
    VRS.refreshManager = new VRS.RefreshManager();
})(VRS || (VRS = {}));
//# sourceMappingURL=refreshManager.js.map