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
using System.Windows;
using System.Windows.Forms;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A layer above <see cref="ControlBinder"/> that takes care of the common
    /// chores involved in binding between a single property on a model that
    /// implements INotifyPropertyChanged and a single property on a control.
    /// </summary>
    public abstract class ValueBinder<TModel, TControl, TValue> : ControlBinder
        where TModel: class, INotifyPropertyChanged
        where TControl: Control
    {
        #region Properties
        /// <summary>
        /// Gets the model.
        /// </summary>
        public TModel Model
        {
            get { return ModelObject as TModel; }
            set { ChangeModelObject(value); }
        }

        /// <summary>
        /// Gets the control.
        /// </summary>
        public TControl Control
        {
            get { return ControlObject as TControl; }
        }

        private Expression<Func<TModel, TValue>> _GetModelValueExpression;
        private Func<TModel, TValue> _GetModelValue;
        private Action<TModel, TValue> _SetModelValue;
        /// <summary>
        /// Gets the model's value.
        /// </summary>
        public TValue ModelValue
        {
            get             { return _GetModelValue(Model); }
            protected set   { if(Model != null) _SetModelValue(Model, value); }
        }

        /// <summary>
        /// Protected method that reads a control's current value.
        /// </summary>
        protected Func<TControl, TValue> _GetControlValue;

        /// <summary>
        /// Protected method that writes a control's current value.
        /// </summary>
        protected Action<TControl, TValue> _SetControlValue;

        /// <summary>
        /// Gets the control's value.
        /// </summary>
        public TValue ControlValue
        {
            get             { return _GetControlValue(Control); }
            protected set   { _SetControlValue(Control, value); }
        }

        private string _ModelPropertyName;
        /// <summary>
        /// Gets or sets the name of the model's property to bind to.
        /// Only required if the ctor getModelValue parameter does more than just return a property.
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
            get             { return ModelValue; }
            protected set   { ModelValue = (TValue)value; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ControlValueObject
        {
            get             { return ControlValue; }
            protected set   { ControlValue = (TValue)value; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        public ValueBinder(
            TModel model, TControl control,
            Expression<Func<TModel, TValue>> getModelValue, Action<TModel, TValue> setModelValue)
            : this(model, control, getModelValue, setModelValue, null, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelValue"></param>
        /// <param name="setModelValue"></param>
        /// <param name="getControlValue"></param>
        /// <param name="setControlValue"></param>
        public ValueBinder(
            TModel model, TControl control,
            Expression<Func<TModel, TValue>> getModelValue, Action<TModel, TValue> setModelValue,
            Func<TControl, TValue> getControlValue, Action<TControl, TValue> setControlValue)
            : base(model, control)
        {
            _GetModelValueExpression = getModelValue;
            _GetModelValue = getModelValue.Compile();
            _SetModelValue = setModelValue;
            _GetControlValue = getControlValue;
            _SetControlValue = setControlValue;
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            base.DoInitialising();

            if(String.IsNullOrEmpty(_ModelPropertyName)) {
                _ModelPropertyName = PropertyHelper.ExtractName(_GetModelValueExpression);
            }
        }
        #endregion

        #region DoHookModel, DoUnhookModel
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoHookModel()
        {
            base.DoHookModel();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookModel()
        {
            Model.PropertyChanged -= Model_PropertyChanged;
            base.DoUnhookModel();
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when a property changes on the model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(Object.ReferenceEquals(sender, ModelObject) && e != null && e.PropertyName == ModelPropertyName) {
                CopyModelToControl();
            }
        }
        #endregion
    }
}
