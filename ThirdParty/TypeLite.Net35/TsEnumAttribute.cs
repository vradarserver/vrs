using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite {
	/// <summary>
	/// Configures an enum to be included in the script model.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	public sealed class TsEnumAttribute : Attribute {
		/// <summary>
		/// Gets or sets the name of the enum in the script model. If it isn't set, the name of the CLR enum is used.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets name of the module for the enum. If it isn't set, the namespace is used.
		/// </summary>
		public string Module { get; set; }
	}
}
