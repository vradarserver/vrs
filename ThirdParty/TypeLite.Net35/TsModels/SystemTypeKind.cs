using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Defines kind of the system type.
	/// </summary>
	public enum SystemTypeKind {
		/// <summary>
		/// Number
		/// </summary>
		Number = 1,

		/// <summary>
		/// String
		/// </summary>
		String = 2,

		/// <summary>
		/// Boolean
		/// </summary>
		Bool = 3,

		/// <summary>
		/// Date
		/// </summary>
		Date = 4
	}
}
