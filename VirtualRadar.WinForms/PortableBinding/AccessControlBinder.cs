// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder between an <see cref="Access"/> object and an <see cref="AccessControl"/> control.
    /// </summary>
    class AccessControlBinder<TModel> : ControlBinder
    {
        #region Fields
        #endregion

        #region Properties
        /// <summary>
        /// Gets the control that we're bound to.
        /// </summary>
        public AccessControl Control
        {
            get { return (AccessControl)ControlObject; }
        }

        /// <summary>
        /// Gets the model that we're bound to.
        /// </summary>
        public TModel Model
        {
            get { return (TModel)ModelObject; }
        }

        private Func<TModel, Access> _GetModelAccess;
        /// <summary>
        /// Gets the <see cref="Access"/> exposed by the model.
        /// </summary>
        public Access Access
        {
            get             { return _GetModelAccess(Model); }
        }

        private string _ModelPropertyName;
        /// <summary>
        /// Gets or sets the name of the model's property to bind to.
        /// Only required if the ctor getModelAccess parameter does more than just return a property.
        /// Cannot be set once the binder has been initialised.
        /// </summary>
        public string ModelPropertyName
        {
            get { return _ModelPropertyName; }
            set { if(!Initialised) _ModelPropertyName = value; }
        }
        #endregion

        #region ControlBinder Properties - ModelValueObject, ControlValueObject
        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ModelValueObject
        {
            get             { return Access; }
            protected set   { ; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ControlValueObject
        {
            get             { return null; }
            protected set   { ; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelAccess"></param>
        public AccessControlBinder(TModel model, AccessControl control, Func<TModel, Access> getModelAccess) : base(model, control)
        {
            _GetModelAccess = getModelAccess;
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            base.DoInitialising();
        }
        #endregion

        #region DoHookModel, DoUnhookModel, DoHookControl, DoUnhookControl
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoHookModel()
        {
            base.DoHookModel();
            Access.PropertyChanged += Access_PropertyChanged;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookModel()
        {
            Access.PropertyChanged -= Access_PropertyChanged;
            base.DoUnhookModel();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoHookControl()
        {
            Control.AddressesChanged += Control_AddressesChanged;
            Control.DefaultAccessChanged += Control_DefaultAccessChanged;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookControl()
        {
            Control.DefaultAccessChanged -= Control_DefaultAccessChanged;
            Control.AddressesChanged -= Control_AddressesChanged;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoHookControlPropertyChanged(EventHandler eventHandler)
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoUnhookControlPropertyChanged(EventHandler eventHandler)
        {
            ;
        }
        #endregion

        #region DoCopy
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            Control.DefaultAccess = Access.DefaultAccess;
            Control.RefreshAddresses(Access.Addresses);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyControlToModel()
        {
            Access.DefaultAccess = Control.DefaultAccess;

            Access.Addresses.Clear();
            Access.Addresses.AddRange(Control.Addresses);
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the properties on the <see cref="Access"/> change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Access_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CopyModelToControl();
        }

        /// <summary>
        /// Called when the default access changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_AddressesChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }

        /// <summary>
        /// Called when the CIDR list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_DefaultAccessChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }
        #endregion
    }
}
