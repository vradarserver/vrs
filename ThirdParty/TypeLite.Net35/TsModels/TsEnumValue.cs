using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a value of the enum
	/// </summary>
	public class TsEnumValue {
		/// <summary>
		/// Gets or sets name of the enum value
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets or sets value of the enum
		/// </summary>
		public string Value { get; private set; }

        public FieldInfo Field { get; private set; }
        
        /// <summary>
		/// Initializes a new instance of the TsEnumValue class.
		/// </summary>
		public TsEnumValue() {
		}

		/// <summary>
		/// Initializes a new instance of the TsEnumValue class with the specific name and value.
		/// </summary>
		/// <param name="name">The name of the enum value.</param>
		/// <param name="value">The value of the enum value.</param>
        public TsEnumValue(FieldInfo field) {
            this.Field = field;
            this.Name = field.Name;
		    
            var value = field.GetValue(null);

		    var valueType = Enum.GetUnderlyingType(value.GetType());
			if (valueType == typeof(byte)) {
				this.Value = ((byte)value).ToString();
			}
			if (valueType == typeof(sbyte)) {
				this.Value = ((sbyte)value).ToString();
			}
			if (valueType == typeof(short)) {
				this.Value = ((short)value).ToString();
			}
			if (valueType == typeof(ushort)) {
				this.Value = ((ushort)value).ToString();
			}
			if (valueType == typeof(int)) {
				this.Value = ((int)value).ToString();
			}
			if (valueType == typeof(uint)) {
				this.Value = ((uint)value).ToString();
			}
			if (valueType == typeof(long)) {
				this.Value = ((long)value).ToString();
			}
			if (valueType == typeof(ulong)) {
				this.Value = ((ulong)value).ToString();
			}
		}
	}
}
