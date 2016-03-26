using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Defines a method used to convert specific Type to the string representation.
	/// </summary>
	/// <param name="type">The type to convert.</param>
	/// <returns>The string representation of the type.</returns>
	public delegate string TypeConvertor(object type);
}
