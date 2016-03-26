using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Defines a method used to format class member types.
	/// </summary>
	/// <param name="tsProperty">The typescript property</param>
	/// <returns>The formatted type.</returns>
	public delegate string TsMemberTypeFormatter(TsProperty tsProperty, string memberTypeName);

	/// <summary>
	/// Defines a method used to format class member types.
	/// </summary>
	/// <param name="memberTypeName">The type name to format</param>
	/// <param name="isMemberCollection">Indicates if member is collection</param>
	/// <param name="collectionDimension">The dimension of the collection - 1 for an array, 2 for an array of arrays etc</param>
	/// <returns>The formatted type.</returns>
	[Obsolete]
	public delegate string TsSimpleMemberTypeFormatter(string memberTypeName, bool isMemberCollection, int collectionDimension = 1);
}
