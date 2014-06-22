using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// The base for all objects that can bind observables to controls.
    /// </summary>
    public abstract class Binder<TControl> : IBinder
        where TControl: Control
    {
        #region Fields
        /// <summary>
        /// True if the Changed event on the control is not to result in the value being
        /// copied back to the observable.
        /// </summary>
        protected bool _SuppressCopyBackOnControlChanged;

        /// <summary>
        /// True if the Changed event on the observable is not to result in the value
        /// being copied back to the control.
        /// </summary>
        protected bool _SuppressCopyBackOnObservableChanged;

        /// <summary>
        /// True if the value changed event has been hooked.
        /// </summary>
        protected bool _HookedControlChanged;

        /// <summary>
        /// True if the value changed event on the observable has been hooked.
        /// </summary>
        protected bool _HookedObservableChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the observable that we're binding values to.
        /// </summary>
        public IObservable Observable { get; private set; }

        /// <summary>
        /// Gets the control whose content is coming from the observable.
        /// </summary>
        public TControl Control { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        Control IBinder.Control
        {
            get { return this.Control; }
        }
        #endregion

        #region Ctor and finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="control"></param>
        public Binder(IObservable observable, TControl control)
        {
            Observable = observable;
            Control = control;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Binder()
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
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                DoUnhookControlChanged();
                DoUnhookObservableChanged();
            }
        }
        #endregion

        #region SuppressCopyBackOnControlChanged, SuppressCopyBackOnObservableChanged
        /// <summary>
        /// Sets a value indicating that the copy back to the observable when the control
        /// changes should be suppressed. Returns the old setting for the value.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool SuppressCopyBackOnControlChanged(bool newValue)
        {
            var result = _SuppressCopyBackOnControlChanged;
            _SuppressCopyBackOnControlChanged = newValue;
            return result;
        }

        /// <summary>
        /// Sets a value indicating that the copy back to the control when the observable
        /// changes should be suppressed. Returns the old setting for the value.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool SuppressCopyBackOnObservableChanged(bool newValue)
        {
            var result = _SuppressCopyBackOnObservableChanged;
            _SuppressCopyBackOnObservableChanged = newValue;
            return result;
        }
        #endregion

        #region SetControlFromObservable, SetObservableFromControl
        /// <summary>
        /// Copies the observable's value to the control.
        /// </summary>
        protected abstract void SetControlFromObservable();

        /// <summary>
        /// Copies the control's value to the observable.
        /// </summary>
        protected abstract void SetObservableFromControl();
        #endregion

        #region DoHookControlChangedEvent, DoUnhookControlChanged, DoHookObservableChanged, DoUnhookObservableChanged
        /// <summary>
        /// Hooks the value changed event for the control.
        /// </summary>
        private void DoHookControlChangedEvent()
        {
            if(!_HookedControlChanged) {
                HookControlChanged();
                _HookedControlChanged = true;
            }
        }

        /// <summary>
        /// Hooks the value changed event.
        /// </summary>
        protected abstract void HookControlChanged();

        /// <summary>
        /// Unhooks the control value changed event.
        /// </summary>
        private void DoUnhookControlChanged()
        {
            if(_HookedControlChanged) {
                UnhookControlChanged();
                _HookedControlChanged = false;
            }
        }

        /// <summary>
        /// Unhooks the value changed event.
        /// </summary>
        protected abstract void UnhookControlChanged();

        /// <summary>
        /// Hooks the observable's value changed event.
        /// </summary>
        private void DoHookObservableChanged()
        {
            if(!_HookedObservableChanged) {
                Observable.Changed += Observable_ValueChanged;
                _HookedObservableChanged = true;
            }
        }

        /// <summary>
        /// Unhooks the observable's value changed event.
        /// </summary>
        private void DoUnhookObservableChanged()
        {
            if(_HookedObservableChanged) {
                Observable.Changed -= Observable_ValueChanged;
                _HookedObservableChanged = false;
            }
        }
        #endregion

        #region InitialiseControl
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void InitialiseControl()
        {
            var suppressCopyFromControl = SuppressCopyBackOnControlChanged(true);
            var suppressCopyFromObservable = SuppressCopyBackOnObservableChanged(true);

            try {
                SetControlFromObservable();
                DoHookControlChangedEvent();
                DoHookObservableChanged();
            } finally {
                SuppressCopyBackOnControlChanged(suppressCopyFromControl);
                SuppressCopyBackOnObservableChanged(suppressCopyFromObservable);
            }
        }
        #endregion

        #region GetControlValue
        /// <summary>
        /// Returns the value of the control. Only needs to be implemented if
        /// <see cref="ControlValueEqualsObservableValue"/> is not implemented.
        /// </summary>
        /// <returns></returns>
        protected virtual object GetControlValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the result of a comparison between the observable value and the control value.
        /// </summary>
        /// <param name="fromControlToObservable"></param>
        /// <returns></returns>
        protected virtual bool ControlValueEqualsObservableValue(bool fromControlToObservable)
        {
            var controlValue = GetControlValue();
            var observableValue = Observable.GetValue();

            return Object.Equals(controlValue, observableValue);
        }
        #endregion

        #region RefreshControl
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshControl()
        {
            SetControlFromObservable();
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the control's value has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void Control_ValueChanged(object sender, EventArgs args)
        {
            if(!_SuppressCopyBackOnControlChanged) {
                if(!ControlValueEqualsObservableValue(true)) {
                    SetObservableFromControl();
                }
            }
        }

        /// <summary>
        /// Called when the observable's value has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void Observable_ValueChanged(object sender, EventArgs args)
        {
            if(!_SuppressCopyBackOnObservableChanged) {
                if(!ControlValueEqualsObservableValue(false)) {
                    SetControlFromObservable();
                }
            }
        }
        #endregion
    }
}
