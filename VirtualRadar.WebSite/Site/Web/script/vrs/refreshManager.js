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

(function(VRS, $, /** object= */ undefined)
{
    //region RefreshTarget
    /**
     * Describes a target held by the refresh manager.
     * @param {Object}      settings
     * @param {jQuery}      settings.targetJQ       The element of the target.
     * @param {function()}  settings.onRefresh      The method to call when the target needs to be refreshed.
     * @param {Object}     [settings.onRefreshThis] The object to use as 'this' when the target is refreshed.
     * @constructor
     */
    VRS.RefreshTarget = function(settings)
    {
        var that = this;

        /**
         * The target jQuery element.
         * @type {jQuery}
         */
        this.targetJQ = settings.targetJQ;

        /**
         * The function to call when it has been determined that the target needs to be refreshed.
         * @type {function()}
         */
        this.onRefresh = settings.onRefresh;

        /**
         * The object to use as 'this' when calling onRefresh.
         * @type {*}
         */
        this.onRefreshThis = settings.onRefreshThis || window;

        /**
         * Ensures that any resources held by the object are released.
         */
        this.dispose = function()
        {
            that.targetJQ = null;
            that.onRefresh = null;
            that.onRefreshThis = null;
        };

        /**
         * Calls onRefresh with the appropriate this parameter.
         */
        this.callOnRefresh = function()
        {
            that.onRefresh.call(that.onRefreshThis);
        };
    };
    //endregion

    //region RefreshOwner
    /**
     * Describes the owner of a list of targets. More than one owner could potentially own a target, and owners can also
     * be targets of something else.
     * @param {Object}                      settings
     * @param {jQuery}                      settings.ownerJQ        The element of the owner.
     * @param {Array.<VRS.RefreshTarget>}  [settings.targets]       The array of targets to initialise with.
     * @constructor
     */
    VRS.RefreshOwner = function(settings)
    {
        var that = this;

        /**
         * The owner jQuery element.
         * @type {jQuery}
         */
        this.ownerJQ = settings.ownerJQ;

        /**
         * An array of targets that have to be refreshed when the owner indicates that something has happened.
         * @type {Array.<VRS.RefreshTarget>}
         */
        var _Targets = settings.targets || [];

        /**
         * Ensures that any resources held by the object are released.
         */
        this.dispose = function()
        {
            that.ownerJQ = null;
            _Targets = [];
        };

        /**
         * Causes all of the targets to be refreshed.
         */
        this.refreshTargets = function()
        {
            var length = _Targets.length;
            for(var i = 0;i < length;++i) {
                _Targets[i].callOnRefresh();
            }
        };

        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or null if no such target exists.
         * @param {jQuery} elementJQ
         * @returns {VRS.RefreshTarget}
         */
        this.getTarget = function(elementJQ)
        {
            var index = that.getTargetIndex(elementJQ);
            return index === -1 ? null : _Targets[index];
        };

        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or -1 if no such target exists.
         * @param {jQuery} elementJQ
         * @returns {number}
         */
        this.getTargetIndex = function(elementJQ)
        {
            var result = -1;

            var length = _Targets.length;
            for(var i = 0;i < length;++i) {
                var target = _Targets[i];
                if(target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        };

        /**
         * Adds the target to the list of known targets for the owner.
         * @param {VRS.RefreshTarget} target
         */
        this.addTarget = function(target)
        {
            if(that.getTargetIndex(target.targetJQ) === -1) {
                _Targets.push(target);
            }
        };

        /**
         * Removes the target from the list of known targets for the owner.
         * @param {VRS.RefreshTarget} target
         */
        this.removeTarget = function(target)
        {
            var index = that.getTargetIndex(target.targetJQ);
            if(index !== -1) _Targets.splice(index, 1);
        };

        /**
         * Removes all targets from the owner.
         */
        this.removeAllTargets = function()
        {
            _Targets = [];
        };
    };
    //endregion

    //region RefreshManager
    /**
     * The object that can record maintain lists of targets and owners and ensure that the correct targets are told to
     * refresh themselves when an owner indicates that they need to do so.
     * @constructor
     */
    VRS.RefreshManager = function()
    {
        //region -- Fields
        var that = this;

        /**
         * An array of every targets known to the manager.
         * @type {Array.<VRS.RefreshTarget>}
         * @private
         */
        var _Targets = [];

        /**
         * An array of every owner known to the manager.
         * @type {Array.<VRS.RefreshOwner>}
         * @private
         */
        var _Owners = [];
        //endregion

        //region -- registerTarget, unregisterTarget
        /**
         * Registers a target with the manager. It is attached to any existing owners that are a parent of the targetJQ
         * and such any new owner be registered in the future, and the target is a child of the new owner, it automatically
         * gets registered with them.
         * @param {jQuery}      elementJQ       The jQuery element for the target.
         * @param {function()}  onRefresh       The function to call when the target is to be refreshed.
         * @param {*}          [onRefreshThis]  The object to use for 'this' when calling onRefresh.
         */
        this.registerTarget = function(elementJQ, onRefresh, onRefreshThis)
        {
            if(getTargetIndex(elementJQ) === -1) {
                var target = new VRS.RefreshTarget({
                    targetJQ: elementJQ,
                    onRefresh: onRefresh,
                    onRefreshThis: onRefreshThis
                });
                _Targets.push(target);

                var allOwners = buildOwners(elementJQ);
                var length = allOwners.length;
                for(var i = 0;i < length;++i) {
                    var owner = allOwners[i];
                    owner.addTarget(target);    // It is safe to call this even if the target is already registered.
                }
            }
        };

        /**
         * Removes the registration for a target. The onRefresh method will no longer be called for any owners that the
         * target has as a parent.
         * @param {jQuery} elementJQ
         */
        this.unregisterTarget = function(elementJQ)
        {
            var index = getTargetIndex(elementJQ);
            if(index !== -1) {
                var target = _Targets[index];

                var length = _Owners.length;
                for(var i = 0;i < length;++i) {
                    _Owners[i].removeTarget(target);        // It is safe to call this even if the target is not registered to the owner.
                }

                _Targets.splice(index, 1);
                target.dispose();
            }
        };
        //endregion

        //region -- registerOwner, unregisterOwner
        /**
         * Registers an element as an owner. Any existing targets that have this element as a parent are automatically
         * registered as targets for the owner, and any future targets registered that have this element as the parent
         * will automatically become attached to the owner.
         * @param {jQuery} elementJQ    The jQuery element for the owner.
         */
        this.registerOwner = function(elementJQ)
        {
            if(getOwnerIndex(elementJQ) === -1) {
                var targets = [];
                var targetLength = _Targets.length;
                for(var i = 0;i < targetLength;++i) {
                    var target = _Targets[i];
                    target.targetJQ.parents().each(function(/** number */ idx, /** HTMLElement */ parentElement) {
                        var continueEach = true;
                        if(elementJQ.is(parentElement)) {
                            targets.push(target);
                            continueEach = false;
                        }
                        return continueEach;
                    })
                }

                var owner = new VRS.RefreshOwner({
                    ownerJQ: elementJQ,
                    targets: targets
                });
                _Owners.push(owner);
            }
        };

        /**
         * Removes the registration of an owner. Any targets attached to the owner will no longer be attached and the
         * owner is forgotten about.
         * @param {jQuery} elementJQ
         */
        this.unregisterOwner = function(elementJQ)
        {
            var index = getOwnerIndex(elementJQ);
            if(index !== -1) {
                var owner = _Owners[index];
                owner.dispose();
                _Owners.splice(index, 1);
            }
        };
        //endregion

        //region -- rebuildRelationships
        /**
         * Forces a complete rebuild of all of the relationships between existing targets and owners. This should be
         * called whenever something moves DOM elements in such a way that existing parent/child relationships could be
         * distrupted.
         */
        this.rebuildRelationships = function()
        {
            var length;
            var i;

            length = _Owners.length;
            for(i = 0;i < length;++i) {
                _Owners[i].removeAllTargets();
            }

            length = _Targets.length;
            for(i = 0;i < length;++i) {
                var target = _Targets[i];
                var owners = buildOwners(target.targetJQ);
                var ownersLength = owners.length;
                for(var j = 0;j < ownersLength;++j) {
                    owners[j].addTarget(target);
                }
            }
        };
        //endregion

        //region -- refreshTargets
        /**
         * Refreshes all of the targets associated with an owner.
         * @param {jQuery} ownerJQ      The owner element whose targets are to be refreshed.
         */
        this.refreshTargets = function(ownerJQ)
        {
            var index = getOwnerIndex(ownerJQ);
            if(index !== -1) _Owners[index].refreshTargets();
        };
        //endregion

        //region -- getTargetIndex, getOwnerIndex
        /**
         * Returns the VRS.RefreshTarget for the jQuery element passed across or -1 if no such target exists.
         * @param {jQuery} elementJQ
         * @returns {number}
         */
        function getTargetIndex(elementJQ)
        {
            var result = -1;

            var length = _Targets.length;
            for(var i = 0;i < length;++i) {
                var target = _Targets[i];
                if(target.targetJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Returns the VRS.RefreshOwner for the jQuery element passed across or -1 if no such owner exists.
         * @param {jQuery} elementJQ
         * @returns {number}
         */
        function getOwnerIndex(elementJQ)
        {
            var result = -1;

            var length = _Owners.length;
            for(var i = 0;i < length;++i) {
                var owner = _Owners[i];
                if(owner.ownerJQ.is(elementJQ)) {
                    result = i;
                    break;
                }
            }

            return result;
        }
        //endregion

        //region -- buildOwners, buildTargets
        /**
         * Examines all of the parents of the element passed across to build a list of registered VRS.RefreshOwner objects.
         * It does not take into account the targets already registered against the owner, the targets held by an owner
         * object may or may not include the element passed across.
         * @param {jQuery} elementJQ
         * @returns {Array.<VRS.RefreshOwner>}
         */
        function buildOwners(elementJQ)
        {
            var result = [];

            var parents = elementJQ.parents();

            var ownerLength = _Owners.length;
            for(var i = 0;i < ownerLength;++i) {
                var owner = _Owners[i];

                parents.each(function(/** number */idx, /** HTMLElement */ parentElement) {
                    if(owner.ownerJQ.is(parentElement)) result.push(owner);
                });
            }

            return result;
        }
        //endregion
    };

    /**
     * The singleton instance of the refresh manager.
     * @type {VRS.RefreshManager}
     */
    VRS.refreshManager = new VRS.RefreshManager();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));