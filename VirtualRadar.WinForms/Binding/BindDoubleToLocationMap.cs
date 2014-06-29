using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.Binding
{
    /// <summary>
    /// Binds a double to either the latitude or longitude on a location map control.
    /// </summary>
    public class BindDoubleToLocationMap : Binder<LocationMapControl>
    {
        public Observable<double> CastObservable { get { return (Observable<double>)Observable; } }

        public bool BoundLatitude { get; private set; }

        public BindDoubleToLocationMap(Observable<double> observable, LocationMapControl control, string propertyName) : base(observable, control)
        {
            BoundLatitude = propertyName == "Latitude";
        }

        protected override object GetControlValue()
        {
            return BoundLatitude ? Control.Latitude : Control.Longitude;
        }

        protected override void SetControlFromObservable()
        {
            if(BoundLatitude) Control.Latitude = CastObservable.Value;
            else              Control.Longitude = CastObservable.Value;
        }

        protected override void SetObservableFromControl()
        {
            CastObservable.Value = (double)GetControlValue();
        }

        protected override void HookControlChanged()
        {
            Control.ValueChanged += Control_ValueChanged;
        }

        protected override void UnhookControlChanged()
        {
            Control.ValueChanged -= Control_ValueChanged;
        }
    }
}
