/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview Code that handles propagation of refresh events to targets.
 */

namespace VRS
{
    /**
     * The settings that can be passed when creating a new instance of RefreshTarget
     */
    export interface RefreshTarget_Settings
    {
        /**
         * The element of the target.
         */
        targetJQ: JQuery;

        /**
         * The method to call when the target needs to be refreshed.
         */
        onRefresh: () => void;

        /**
         * The object to set 'this' to when calling onRefresh.
         */
        onRefreshThis?: Object;
    }

    export class RefreshTarget
    {
        // Kept as public fields for backwards compatibility
        targetJQ:       JQuery;
        onRefresh:      () => void;
        onRefreshThis:  Object;

        constructor(settings: RefreshTarget_Settings)
        {
            this.targetJQ = settings.targetJQ;
            this.onRefresh = settings.onRefresh;
            this.onRefreshThis = settings.onRefreshThis || window;
        }

        /**
         * Ensures that any resources held by the object are released.
         */
        dispose()
        {
            this.targetJQ = null;
            this.onRefresh = null;
            this.onRefreshThis = null;
        }

        /**
         * Calls onRefresh with the appropriate this parameter.
         */
        callOnRefresh = function()
        {
            this.onRefresh.call(this.onRefreshThis);
        }
    }

    /**
     * The settings that need to be passed in when creating a new instance of RefreshOwner
     */
    export interface RefreshOwner_Settings
    {
        /**
         * The element of the owner.
         */
        ownerJQ:    JQuery;

        /**
         * The array of targets to initialise with.
         */
        targets?:   RefreshTarget[];
    }

    /**
     * Describes the owner of a list of targets. More than one owner could potentially own a target, and owners can also
     * be targets of something else.
     */
    export class RefreshOwner
    {
        private _Targets: RefreshTarget[];

        // Kept as public field for backwards compatibility
        ownerJQ: JQuery;

        constructor(settings: RefreshOwner_Settings)
        {
            this.ownerJQ = settings.ownerJQ;
            this._Targets = settings.targets || [];
        }

        /**
         * Ensures that any resources held by the object are released.
         */
        dispose()
        {
            this.ownerJQ = null;
            this._Targets = [];
        }

        /**
         * Causes all of the targets to be refreshed.
         */
        refreshTargets()
        {
            var length = this._Targets.length;
            for(var i = 0;i < length;++i) {
                this._Targets[i].callOnRefresh();
            }
        }

        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or null if no such target exists.
         */
        getTarget(elementJQ: JQuery) : RefreshTarget
        {
            var index = this.getTargetIndex(elementJQ);
            return index === -1 ? null : this._Targets[index];
        };

        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or -1 if no such target exists.
         */
        getTargetIndex(elementJQ: JQuery) : number
        {
            var result = -1;

            var length = this._Targets.length;
            for(var i = 0;i < length;++i) {
                var target = this._Targets[i];
                if(target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Adds the target to the list of known targets for the owner.
         */
        addTarget(target: RefreshTarget)
        {
            if(this.getTargetIndex(target.targetJQ) === -1) {
                this._Targets.push(target);
            }
        }

        /**
         * Removes the target from the list of known targets for the owner.
         */
        removeTarget(target: RefreshTarget)
        {
            var index = this.getTargetIndex(target.targetJQ);
            if(index !== -1) this._Targets.splice(index, 1);
        }

        /**
         * Removes all targets from the owner.
         */
        removeAllTargets()
        {
            this._Targets = [];
        }
    }

    /**
     * The object that can record maintain lists of targets and owners and ensure that the correct targets are told to
     * refresh themselves when an owner indicates that they need to do so.
     */
    export class RefreshManager
    {
        /**
         * An array of every targets known to the manager.
         */
        private _Targets: RefreshTarget[] = [];

        /**
         * An array of every owner known to the manager.
         */
        private _Owners: RefreshOwner[] = [];

        /**
         * Registers a target with the manager. It is attached to any existing owners that are a parent of the targetJQ
         * and such any new owner be registered in the future, and the target is a child of the new owner, it automatically
         * gets registered with them.
         * @param {jQuery}      elementJQ       The jQuery element for the target.
         * @param {function()}  onRefresh       The function to call when the target is to be refreshed.
         * @param {*}          [onRefreshThis]  The object to use for 'this' when calling onRefresh.
         */
        registerTarget(elementJQ: JQuery, onRefresh: () => void, onRefreshThis?: Object)
        {
            if(this.getTargetIndex(elementJQ) === -1) {
                var target = new VRS.RefreshTarget({
                    targetJQ: elementJQ,
                    onRefresh: onRefresh,
                    onRefreshThis: onRefreshThis
                });
                this._Targets.push(target);

                var allOwners = this.buildOwners(elementJQ);
                var length = allOwners.length;
                for(var i = 0;i < length;++i) {
                    var owner = allOwners[i];
                    owner.addTarget(target);    // It is safe to call this even if the target is already registered.
                }
            }
        }

        /**
         * Removes the registration for a target. The onRefresh method will no longer be called for any owners that the
         * target has as a parent.
         */
        unregisterTarget(elementJQ: JQuery)
        {
            var index = this.getTargetIndex(elementJQ);
            if(index !== -1) {
                var target = this._Targets[index];

                var length = this._Owners.length;
                for(var i = 0;i < length;++i) {
                    this._Owners[i].removeTarget(target);        // It is safe to call this even if the target is not registered to the owner.
                }

                this._Targets.splice(index, 1);
                target.dispose();
            }
        }

        /**
         * Registers an element as an owner. Any existing targets that have this element as a parent are automatically
         * registered as targets for the owner, and any future targets registered that have this element as the parent
         * will automatically become attached to the owner.
         */
        registerOwner(elementJQ: JQuery)
        {
            if(this.getOwnerIndex(elementJQ) === -1) {
                var targets = [];
                var targetLength = this._Targets.length;
                for(var i = 0;i < targetLength;++i) {
                    var target = this._Targets[i];
                    target.targetJQ.parents().each(function(idx, parentElement) {
                        var continueEach = true;
                        if(elementJQ.is(parentElement)) {
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
        }

        /**
         * Removes the registration of an owner. Any targets attached to the owner will no longer be attached and the
         * owner is forgotten about.
         */
        unregisterOwner(elementJQ: JQuery)
        {
            var index = this.getOwnerIndex(elementJQ);
            if(index !== -1) {
                var owner = this._Owners[index];
                owner.dispose();
                this._Owners.splice(index, 1);
            }
        }

        /**
         * Forces a complete rebuild of all of the relationships between existing targets and owners. This should be
         * called whenever something moves DOM elements in such a way that existing parent/child relationships could be
         * distrupted.
         */
        rebuildRelationships()
        {
            var length;
            var i;

            length = this._Owners.length;
            for(i = 0;i < length;++i) {
                this._Owners[i].removeAllTargets();
            }

            length = this._Targets.length;
            for(i = 0;i < length;++i) {
                var target = this._Targets[i];
                var owners = this.buildOwners(target.targetJQ);
                var ownersLength = owners.length;
                for(var j = 0;j < ownersLength;++j) {
                    owners[j].addTarget(target);
                }
            }
        }

        /**
         * Refreshes all of the targets associated with an owner.
         */
        refreshTargets(ownerJQ: JQuery)
        {
            var index = this.getOwnerIndex(ownerJQ);
            if(index !== -1) this._Owners[index].refreshTargets();
        }

        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or -1 if no such target exists.
         */
        private getTargetIndex(elementJQ: JQuery) : number
        {
            var result = -1;

            var length = this._Targets.length;
            for(var i = 0;i < length;++i) {
                var target = this._Targets[i];
                if(target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Returns the VRS.RefreshOwner for the jQuery element passed across or -1 if no such owner exists.
         */
        private getOwnerIndex(elementJQ: JQuery) : number
        {
            var result = -1;

            var length = this._Owners.length;
            for(var i = 0;i < length;++i) {
                var owner = this._Owners[i];
                if(owner.ownerJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Examines all of the parents of the element passed across to build a list of registered VRS.RefreshOwner objects.
         * It does not take into account the targets already registered against the owner, the targets held by an owner
         * object may or may not include the element passed across.
         */
        private buildOwners(elementJQ: JQuery) : RefreshOwner[]
        {
            var result: RefreshOwner[] = [];

            var parents = elementJQ.parents();

            var ownerLength = this._Owners.length;
            for(var i = 0;i < ownerLength;++i) {
                var owner = this._Owners[i];

                parents.each(function(/** number */idx, /** HTMLElement */ parentElement) {
                    if(owner.ownerJQ.is(parentElement)) result.push(owner);
                });
            }

            return result;
        }
    }

    /*
     * Prebuilts
     */
    export var refreshManager = new VRS.RefreshManager();
}
