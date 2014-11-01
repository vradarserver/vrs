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
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder between a map control and a set of properties describing a position,
    /// a map type and a zoom level.
    /// </summary>
    public class MapValuesBinder<TModel> : ControlBinder
    {
        #region Properties - Model getters and setters
        private Expression<Func<TModel, double>> _GetLatitudeExpression;
        private Func<TModel, double> _GetLatitude;
        /// <summary>
        /// Gets or sets the delegate that returns the latitude from the model.
        /// </summary>
        public Func<TModel, double> GetLatitude
        {
            get { return _GetLatitude; }
            set { if(!Initialised) _GetLatitude = value; }
        }

        private Action<TModel, double> _SetLatitude;
        /// <summary>
        /// Gets or sets the delegate that sets the latitude on the model.
        /// </summary>
        public Action<TModel, double> SetLatitude
        {
            get { return _SetLatitude; }
            set { if(!Initialised) _SetLatitude = value; }
        }

        private string _LatitudePropertyName;
        /// <summary>
        /// Gets or sets the name of the latitude property on the model.
        /// </summary>
        public string LatitudePropertyName
        {
            get { return _LatitudePropertyName; }
            set { if(!Initialised) _LatitudePropertyName = value; }
        }

        private Expression<Func<TModel, double>> _GetLongitudeExpression;
        private Func<TModel, double> _GetLongitude;
        /// <summary>
        /// Gets or sets the delegate that returns the longitude from the model.
        /// </summary>
        public Func<TModel, double> GetLongitude
        {
            get { return _GetLongitude; }
            set { if(!Initialised) _GetLongitude = value; }
        }

        private Action<TModel, double> _SetLongitude;
        /// <summary>
        /// Gets or sets the delegate that sets the longitude on the model.
        /// </summary>
        public Action<TModel, double> SetLongitude
        {
            get { return _SetLongitude; }
            set { if(!Initialised) _SetLongitude = value; }
        }

        private string _LongitudePropertyName;
        /// <summary>
        /// Gets or sets the name of the longitude property on the model.
        /// </summary>
        public string LongitudePropertyName
        {
            get { return _LongitudePropertyName; }
            set { if(!Initialised) _LongitudePropertyName = value; }
        }

        private Expression<Func<TModel, string>> _GetMapTypeExpression;
        private Func<TModel, string> _GetMapType;
        /// <summary>
        /// Gets or sets the delegate that returns the map type from the model.
        /// </summary>
        public Func<TModel, string> GetMapType
        {
            get { return _GetMapType; }
            set { if(!Initialised) _GetMapType = value; }
        }

        private Action<TModel, string> _SetMapType;
        /// <summary>
        /// Gets or sets the delegate that sets the map type on the model.
        /// </summary>
        public Action<TModel, string> SetMapType
        {
            get { return _SetMapType; }
            set { if(!Initialised) _SetMapType = value; }
        }

        private string _MapTypePropertyName;
        /// <summary>
        /// Gets or sets the name of the map type property on the model.
        /// </summary>
        public string MapTypePropertyName
        {
            get { return _MapTypePropertyName; }
            set { if(!Initialised) _MapTypePropertyName = value; }
        }

        private Expression<Func<TModel, int>> _GetZoomLevelExpression;
        private Func<TModel, int> _GetZoomLevel;
        /// <summary>
        /// Gets or sets the delegate that returns the zoom level from the model.
        /// </summary>
        public Func<TModel, int> GetZoomLevel
        {
            get { return _GetZoomLevel; }
            set { if(!Initialised) _GetZoomLevel = value; }
        }

        private Action<TModel, int> _SetZoomLevel;
        /// <summary>
        /// Gets or sets the delegate that sets the zoom level on the model.
        /// </summary>
        public Action<TModel, int> SetZoomLevel
        {
            get { return _SetZoomLevel; }
            set { if(!Initialised) _SetZoomLevel = value; }
        }

        private string _ZoomLevelPropertyName;
        /// <summary>
        /// Gets or sets the name of the zoom level property on the model.
        /// </summary>
        public string ZoomLevelPropertyName
        {
            get { return _ZoomLevelPropertyName; }
            set { if(!Initialised) _ZoomLevelPropertyName = value; }
        }
        #endregion

        #region Model, Control, Model Properties
        /// <summary>
        /// Gets the Model.
        /// </summary>
        public TModel Model
        {
            get { return (TModel)ModelObject; }
        }

        /// <summary>
        /// Gets the control.
        /// </summary>
        public MapControl Control
        {
            get { return (MapControl)ControlObject; }
        }

        /// <summary>
        /// Gets the model as an INotifyPropertyChanged.
        /// </summary>
        public INotifyPropertyChanged ModelNofityPropertyChanged
        {
            get { return ModelObject as INotifyPropertyChanged; }
        }

        /// <summary>
        /// Gets the model latitude.
        /// </summary>
        public double? ModelLatitude
        {
            get             { return GetLatitude == null ? (double?)null : GetLatitude(Model); }
            protected set   { if(SetLatitude != null && value != null) SetLatitude(Model, value.Value); }
        }

        /// <summary>
        /// Gets the model longitude.
        /// </summary>
        public double? ModelLongitude
        {
            get             { return GetLongitude == null ? (double?)null : GetLongitude(Model); }
            protected set   { if(SetLongitude != null && value != null) SetLongitude(Model, value.Value); }
        }

        /// <summary>
        /// Gets the model map type.
        /// </summary>
        public string ModelMapType
        {
            get             { return GetMapType == null ? null : GetMapType(Model); }
            protected set   { if(SetMapType != null && value != null) SetMapType(Model, value); }
        }

        /// <summary>
        /// Gets the model zoom level.
        /// </summary>
        public int? ModelZoomLevel
        {
            get             { return GetZoomLevel == null ? (int?)null : GetZoomLevel(Model); }
            protected set   { if(SetZoomLevel != null && value != null) SetZoomLevel(Model, value.Value); }
        }
        #endregion

        #region ControlBinder Properties - ModelValueObject, ControlValueObject
        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ModelValueObject
        {
            get             { return null; }
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
        /// <param name="getLatitude"></param>
        /// <param name="setLatitude"></param>
        /// <param name="getLongitude"></param>
        /// <param name="setLongitude"></param>
        /// <param name="getMapType"></param>
        /// <param name="setMapType"></param>
        /// <param name="getZoomLevel"></param>
        /// <param name="setZoomLevel"></param>
        public MapValuesBinder(TModel model, MapControl control,
            Expression<Func<TModel, double>> getLatitude,           Action<TModel, double> setLatitude,
            Expression<Func<TModel, double>> getLongitude,          Action<TModel, double> setLongitude,
            Expression<Func<TModel, string>> getMapType = null,     Action<TModel, string> setMapType = null,
            Expression<Func<TModel, int>>    getZoomLevel = null,   Action<TModel, int>    setZoomLevel = null)
            : base(model, control)
        {
            _GetLatitudeExpression = getLatitude;
            _SetLatitude = setLatitude;

            _GetLongitudeExpression = getLongitude;
            _SetLongitude = setLongitude;

            _GetMapTypeExpression = getMapType;
            _SetMapType = setMapType;

            _GetZoomLevelExpression = getZoomLevel;
            _SetZoomLevel = setZoomLevel;
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialiseControl()
        {
            if(_GetLatitudeExpression != null &&    _GetLatitude == null)   _GetLatitude =  _GetLatitudeExpression.Compile();
            if(_GetLongitudeExpression != null &&   _GetLongitude == null)  _GetLongitude = _GetLongitudeExpression.Compile();
            if(_GetMapTypeExpression != null &&     _GetMapType == null)    _GetMapType =   _GetMapTypeExpression.Compile();
            if(_GetZoomLevelExpression != null &&   _GetZoomLevel == null)  _GetZoomLevel = _GetZoomLevelExpression.Compile();

            if(_GetLatitudeExpression != null &&    _LatitudePropertyName == null)  _LatitudePropertyName = PropertyHelper.ExtractName(_GetLatitudeExpression);
            if(_GetLongitudeExpression != null &&   _LongitudePropertyName == null) _LongitudePropertyName = PropertyHelper.ExtractName(_GetLongitudeExpression);
            if(_GetMapTypeExpression != null &&     _MapTypePropertyName == null)   _MapTypePropertyName = PropertyHelper.ExtractName(_GetMapTypeExpression);
            if(_GetZoomLevelExpression != null &&   _ZoomLevelPropertyName == null) _ZoomLevelPropertyName = PropertyHelper.ExtractName(_GetZoomLevelExpression);

            base.DoInitialiseControl();
        }
        #endregion

        #region HookModel, HookControl
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoHookModel()
        {
            if(ModelNofityPropertyChanged != null) {
                ModelNofityPropertyChanged.PropertyChanged += Model_PropertyChanged;
            }
            base.DoHookModel();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookModel()
        {
            if(ModelNofityPropertyChanged != null) {
                ModelNofityPropertyChanged.PropertyChanged -= Model_PropertyChanged;
            }
            base.DoUnhookModel();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoHookControl()
        {
            Control.LatitudeChanged += Control_LatitudeChanged;
            Control.LongitudeChanged += Control_LongitudeChanged;
            Control.MapTypeChanged += Control_MapTypeChanged;
            Control.ZoomLevelChanged += Control_ZoomLevelChanged;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookControl()
        {
            Control.LatitudeChanged -= Control_LatitudeChanged;
            Control.LongitudeChanged -= Control_LongitudeChanged;
            Control.MapTypeChanged -= Control_MapTypeChanged;
            Control.ZoomLevelChanged -= Control_ZoomLevelChanged;
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

        #region CopyModelToControl, CopyControlToModel
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            var latitude = ModelLatitude;
            var longitude = ModelLongitude;
            var mapType = ModelMapType;
            var zoomLevel = ModelZoomLevel;

            if(latitude != null && longitude != null) {
                Control.Latitude = latitude.Value;
                Control.Longitude = longitude.Value;
            }
            if(mapType != null) Control.MapType = mapType;
            if(zoomLevel != null) Control.ZoomLevel = zoomLevel.Value;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyControlToModel()
        {
            if(SetLatitude != null)     ModelLatitude = Control.Latitude;
            if(SetLongitude != null)    ModelLongitude = Control.Longitude;
            if(SetMapType != null)      ModelMapType = Control.MapType;
            if(SetZoomLevel != null)    ModelZoomLevel = Control.ZoomLevel;
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the model changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(!String.IsNullOrEmpty(e.PropertyName)) {
                if(e.PropertyName == _LatitudePropertyName)         CopyModelToControl();
                else if(e.PropertyName == _LongitudePropertyName)   CopyModelToControl();
                else if(e.PropertyName == _MapTypePropertyName)     CopyModelToControl();
                else if(e.PropertyName == _ZoomLevelPropertyName)   CopyModelToControl();
            }
        }

        /// <summary>
        /// Called when the control changes its latitude.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_LatitudeChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }

        /// <summary>
        /// Called when the control changes its longitude.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_LongitudeChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }

        /// <summary>
        /// Called when the control changes its map type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_MapTypeChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }

        /// <summary>
        /// Called when the control changes its zoom level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_ZoomLevelChanged(object sender, EventArgs e)
        {
            CopyControlToModel();
        }
        #endregion
    }
}
