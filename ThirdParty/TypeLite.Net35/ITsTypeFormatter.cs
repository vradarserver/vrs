using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Formats TsType for output.
	/// </summary>
	public interface ITsTypeFormatter {
		/// <summary>
		/// Formats TsType for output
		/// </summary>
		/// <param name="type">The type to format.</param>
		/// <returns>The string representation of the type.</returns>
		string FormatType(TsType type);
	}

}
