using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeLite.Extensions;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents an enum in the code model.
	/// </summary>
	public class TsEnum : TsModuleMember {
		/// <summary>
		/// Gets or sets bool value indicating whether this enum will be ignored by TsGenerator.
		/// </summary>
		public bool IsIgnored { get; set; }

		/// <summary>
		/// Gets collection of properties of the class.
		/// </summary>
		public ICollection<TsEnumValue> Values { get; private set; }

		/// <summary>
		/// Initializes a new instance of the TsEnum class with the specific CLR enum.
		/// </summary>
		/// <param name="type">The CLR enum represented by this instance of the TsEnum.</param>
		public TsEnum(Type type)
			: base(type) {
			if (!this.Type.IsEnum) {
				throw new ArgumentException("ClrType isn't enum.");
			}

			this.Values = new List<TsEnumValue>(this.GetEnumValues(type));

			var attribute = this.Type.GetCustomAttribute<TsEnumAttribute>(false);
			if (attribute != null) {
				if (!string.IsNullOrEmpty(attribute.Name)) {
					this.Name = attribute.Name;
				}

				if (!string.IsNullOrEmpty(attribute.Module)) {
					this.Module.Name = attribute.Module;
				}
			}
		}

		/// <summary>
		/// Retrieves a collection of possible value of the enum.
		/// </summary>
		/// <param name="enumType">The type of the enum.</param>
		/// <returns>collection of all enum values.</returns>
		protected IEnumerable<TsEnumValue> GetEnumValues(Type enumType) {
			return enumType.GetFields()
				.Where(fieldInfo => fieldInfo.IsLiteral && !string.IsNullOrEmpty(fieldInfo.Name))
				.Select(fieldInfo => new TsEnumValue(fieldInfo));
		}
	}
}