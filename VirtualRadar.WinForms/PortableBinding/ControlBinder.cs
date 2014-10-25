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
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// The base class for all control binders.
    /// </summary>
    /// <remarks><para>
    /// This takes care of the basics of binding models to controls. It tries not make too
    /// many assumptions about the nature of the binding - that's for derived classes to do.
    /// </para><para>
    /// ControlBinders have optional properties that can be configured before use - once
    /// <see cref="Initialise"/> has been called these properties can no longer be set.
    /// You can make multiple calls on <see cref="Initialise"/> but the second and subsequent
    /// calls do nothing.
    /// </para><para>
    /// ControlBinders must be disposed of in order to release the event hooks. If you do not
    /// dispose of them then the controls that they are hooking will remain in memory for the
    /// lifetime of the model. Forms and UserControls based on <see cref="BaseForm"/> and
    /// <see cref="BaseUserControl"/> automatically dispose of any ControlBinders that have
    /// been registered with AddControlBinder.
    /// </para>
    /// </remarks>
    public abstract class ControlBinder : IDisposable
    {
        #region Fields
        /// <summary>
        /// True if the model's events have been hooked.
        /// </summary>
        private bool _ModelHooked;

        /// <summary>
        /// True if the control's events have been hooked.
        /// </summary>
        private bool _ControlHooked;

        /// <summary>
        /// True if changes are not to be copied between the control and the model.
        /// </summary>
        protected bool _UpdatesLocked;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating that the object has been initialised.
        /// </summary>
        public bool Initialised { get; private set; }

        /// <summary>
        /// Gets the model that has been bound to the control.
        /// </summary>
        public object ModelObject { get; private set; }

        /// <summary>
        /// Gets the control that has been bound to the model.
        /// </summary>
        public Control ControlObject { get; private set; }

        /// <summary>
        /// Gets the value of the model.
        /// </summary>
        public abstract object ModelValueObject { get; protected set; }

        /// <summary>
        /// Gets the value of the control.
        /// </summary>
        public abstract object ControlValueObject { get; protected set; }

        private DataSourceUpdateMode _UpdateMode = DataSourceUpdateMode.OnValidation;
        /// <summary>
        /// Gets or sets an indication of when values are copied from the control to the model.
        /// Cannot be modified once the binder has been initialised.
        /// </summary>
        public DataSourceUpdateMode UpdateMode
        {
            get { return _UpdateMode; }
            set { if(!Initialised) _UpdateMode = value; }
        }
        #endregion

        #region Ctors, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        public ControlBinder(object model, Control control)
        {
            ModelObject = model;
            ControlObject = control;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~ControlBinder()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalises or disposes of the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                UnhookControl();
                UnhookModel();
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// Initialises the binder. Must be called before the binder will start working.
        /// </summary>
        public void Initialise()
        {
            if(!Initialised) {
                DoInitialising();

                HookModel();
                HookControl();

                InitialiseControl();

                Initialised = true;
            }
        }
        #endregion

        #region HookModel, HookControl, UnhookModel, UnhookControl, Do* methods for hooking and unhooking
        /// <summary>
        /// Hooks the events on the model.
        /// </summary>
        protected void HookModel()
        {
            if(!_ModelHooked) {
                try {
                    DoHookModel();
                } finally {
                    _ModelHooked = true;
                }
            }
        }

        /// <summary>
        /// Performs the actual work of hooking the model. Can be overridden.
        /// </summary>
        protected virtual void DoHookModel()
        {
        }

        /// <summary>
        /// Hooks the events on the control.
        /// </summary>
        protected void HookControl()
        {
            if(!_ControlHooked) {
                try {
                    DoHookControl();
                } finally {
                    _ControlHooked = true;
                }
            }
        }

        /// <summary>
        /// Performs the actual work of hooking the control. Can be overridden.
        /// </summary>
        protected virtual void DoHookControl()
        {
            if(ControlObject != null) {
                switch(UpdateMode) {
                    case DataSourceUpdateMode.OnValidation:
                        ControlObject.Validated += Control_Changed;
                        break;
                    case DataSourceUpdateMode.OnPropertyChanged:
                        DoHookControlPropertyChanged(Control_Changed);
                        break;
                    case DataSourceUpdateMode.Never:
                        break;
                }
            }
        }

        /// <summary>
        /// Unhooks the events on the model.
        /// </summary>
        protected void UnhookModel()
        {
            if(_ModelHooked) {
                try {
                    DoUnhookModel();
                } finally {
                    _ModelHooked = false;
                }
            }
        }

        /// <summary>
        /// Performs the actual work of unhooking the model. Can be overridden.
        /// </summary>
        protected virtual void DoUnhookModel()
        {
        }

        /// <summary>
        /// Unhooks the events on the control.
        /// </summary>
        protected void UnhookControl()
        {
            if(_ControlHooked) {
                try {
                    DoUnhookControl();
                } finally {
                    _ControlHooked = false;
                }
            }
        }

        /// <summary>
        /// Performs the actual work of unhooking the control. Can be overridden.
        /// </summary>
        protected virtual void DoUnhookControl()
        {
            if(ControlObject != null) {
                switch(UpdateMode) {
                    case DataSourceUpdateMode.OnValidation:
                        ControlObject.Validated -= Control_Changed;
                        break;
                    case DataSourceUpdateMode.OnPropertyChanged:
                        DoUnhookControlPropertyChanged(Control_Changed);
                        break;
                    case DataSourceUpdateMode.Never:
                        break;
                }
            }
        }

        /// <summary>
        /// Overridden to hook the control event that is raised as soon as the user changes the control's value.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected abstract void DoHookControlPropertyChanged(EventHandler eventHandler);

        /// <summary>
        /// Overridden to unhook the control event that is raised as soon as the user changes the control's value.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected abstract void DoUnhookControlPropertyChanged(EventHandler eventHandler);
        #endregion

        #region InitialiseControl
        /// <summary>
        /// Initialises the control.
        /// </summary>
        private void InitialiseControl()
        {
            DoInitialiseControl();
        }

        /// <summary>
        /// Called before any initialisation is performed, specifically before anything is hooked.
        /// Can be overridden to perform setup that must be in place before any event handlers can
        /// be called.
        /// </summary>
        protected virtual void DoInitialising()
        {
            ;
        }

        /// <summary>
        /// Does the work of initialising the control. By default this just calls <see cref="CopyModelToControl"/>.
        /// </summary>
        protected virtual void DoInitialiseControl()
        {
            CopyModelToControl();
        }
        #endregion

        #region CopyModelToControl, CopyControlToModel
        /// <summary>
        /// Refreshes the control's content with the content of the model.
        /// </summary>
        public virtual void CopyModelToControl()
        {
            if(!_UpdatesLocked) {
                try {
                    _UpdatesLocked = true;
                    DoCopyModelToControl();
                } finally {
                    _UpdatesLocked = false;
                }
            }
        }

        /// <summary>
        /// Refreshes the model's content with the content of the control.
        /// </summary>
        public virtual void CopyControlToModel()
        {
            if(!_UpdatesLocked) {
                try {
                    _UpdatesLocked = true;
                    DoCopyControlToModel();
                } finally {
                    _UpdatesLocked = false;
                }
            }
        }

        /// <summary>
        /// Does the actual work of copying the value from the model to the control.
        /// </summary>
        protected virtual void DoCopyModelToControl()
        {
            ControlValueObject = ModelValueObject;
        }

        /// <summary>
        /// Does the actual work of copying the value from the control to the model.
        /// </summary>
        protected virtual void DoCopyControlToModel()
        {
            ModelValueObject = ControlValueObject;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the control indicates that the value it is holding has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_Changed(object sender, EventArgs e)
        {
            CopyControlToModel();
        }
        #endregion
    }
}
