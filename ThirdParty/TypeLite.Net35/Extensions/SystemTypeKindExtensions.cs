using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite.Extensions {
	/// <summary>
	/// Contains extensions for SystemTypeKind enum
	/// </summary>
	public static class SystemTypeKindExtensions {
		/// <summary>
		/// Converts SystemTypeKind to the string, that can be used as system type identifier in TypeScript.
		/// </summary>
		/// <param name="type">The value to convert</param>
		/// <returns>system type identifier for TypeScript</returns>
		public static string ToTypeScriptString(this SystemTypeKind type) {
			switch (type) {
				case SystemTypeKind.Date: return "Date";
				case SystemTypeKind.Bool: return "boolean";
			}

			return type.ToString().ToLower();
		}
	}
}
