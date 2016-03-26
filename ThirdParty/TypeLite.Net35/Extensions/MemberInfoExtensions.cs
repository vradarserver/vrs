using System;
using System.Linq;
using System.Reflection;

namespace TypeLite.Extensions {
	/// <summary>
	/// Contains extensions for MemberInfo class
	/// </summary>
	public static class MemberInfoExtensions {
		/// <summary>
		/// Retrieves a custom attribute of a specified type that is applied to a specified member.
		/// </summary>
		/// <typeparam name="TType">The type of attribute to search for.</typeparam>
		/// <param name="memberInfo">The member to inspect.</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
		/// <returns>A custom attribute that matches T, or null if no such attribute is found.</returns>
		public static TType GetCustomAttribute<TType>(this MemberInfo memberInfo, bool inherit) where TType : Attribute {
			return memberInfo.GetCustomAttributes(typeof(TType), inherit).FirstOrDefault() as TType;
		}
	}
}
