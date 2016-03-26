using System;
using System.Linq;
using System.Reflection;

namespace TypeLite.Extensions {
	/// <summary>
	/// Contains extensions for PropertyInfo class
	/// </summary>
	public static class TypeExtensions {
		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a specified type.
		/// </summary>
		/// <typeparam name="TType">The type of attribute to search for.</typeparam>
		/// <param name="type">The type to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
		/// <returns>A custom attribute that matches T, or null if no such attribute is found.</returns>
		public static TType GetCustomAttribute<TType>(this Type type, bool inherit) where TType : Attribute {
			return type.GetCustomAttributes(typeof(TType), inherit).FirstOrDefault() as TType;
		}

		/// <summary>
		/// Determined whether the specific type is nullable value type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>true if the type is nullable value type otherwise false</returns>
		public static bool IsNullable(this Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		/// <summary>
		/// Retrieves underlaying value type of the nullable value type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>The underlaying value type.</returns>
		public static Type GetNullableValueType(this Type type) {
			return type.GetGenericArguments().Single();
		}
	}
}
