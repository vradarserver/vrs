using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// Brings together all of the settings that configure the state history feature.
    /// </summary>
    public class StateHistorySettings : INotifyPropertyChanged
    {
        private bool _Enabled;
        /// <summary>
        /// Gets or sets a value indicating whether state history is being saved.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set { SetField(ref _Enabled, value, nameof(Enabled)); }
        }

        private string _NonStandardFolder;
        /// <summary>
        /// Gets or sets the folder to store the state history in. If this is empty or null then
        /// a default location is used. This is ignored when the SQL Server plugin is used.
        /// </summary>
        public string NonStandardFolder
        {
            get { return _NonStandardFolder; }
            set { SetField(ref _NonStandardFolder, value, nameof(NonStandardFolder)); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public StateHistorySettings()
        {
            _Enabled = true;
            _NonStandardFolder = "";
        }
    }
}
