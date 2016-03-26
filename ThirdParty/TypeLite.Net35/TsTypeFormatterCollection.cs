using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Manages collection of TsTypeFormatters
	/// </summary>
	public class TsTypeFormatterCollection : ITsTypeFormatter {
		internal Dictionary<Type, TsTypeFormatter> _formatters;
		
		/// <summary>
		/// Initializes a new instance of the TsTypeFormatterCollection class
		/// </summary>
		internal TsTypeFormatterCollection() {
			_formatters = new Dictionary<Type, TsTypeFormatter>();
		}

		/// <summary>
		/// Converts the specific type to it's string representation using a formatter registered for the type
		/// </summary>
		/// <param name="type">The type to format.</param>
		/// <returns>The string representation of the type.</returns>
		public string FormatType(TsType type) {
			if (_formatters.ContainsKey(type.GetType())) {
				return _formatters[type.GetType()](type, this);
			} else {
				return "any";
			}
		}

		/// <summary>
		/// Registers the formatter for the specific TsType
		/// </summary>
		/// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
		/// <param name="formatter">The formatter to register</param>
		public void RegisterTypeFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
			_formatters[typeof(TFor)] = formatter;
		}
	}
}
