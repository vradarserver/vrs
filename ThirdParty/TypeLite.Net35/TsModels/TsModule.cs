using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeLite.TsModels {
	/// <summary>
	/// Represents a module in the script model.
	/// </summary>
	public class TsModule {
		private HashSet<TsModuleMember> _members;

		/// <summary>
		/// Gets or sets name of the module.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets collection of classes in the module.
		/// </summary>
		public IEnumerable<TsClass> Classes {
			get {
				return _members.OfType<TsClass>();
			}
		}

		/// <summary>
		/// Gets collection of enums in the module
		/// </summary>
		public IEnumerable<TsEnum> Enums {
			get {
				return _members.OfType<TsEnum>();
			}
		}

		/// <summary>
		/// Gets collection of all members of the module
		/// </summary>
		public IEnumerable<TsModuleMember> Members {
			get {
				return _members;
			}
		}

		/// <summary>
		/// Initializes a new instance of the TsModule class.
		/// </summary>
		public TsModule(string name) {
			_members = new HashSet<TsModuleMember>();
			this.Name = name;
		}

		/// <summary>
		/// Adds a member to this module.
		/// </summary>
		/// <param name="toAdd">The member to add.</param>
		internal void Add(TsModuleMember toAdd) {
			_members.Add(toAdd);

		}

		/// <summary>
		/// Removes a member from this module.
		/// </summary>
		/// <param name="toRemove">The member to remove.</param>
		internal void Remove(TsModuleMember toRemove) {
			_members.Remove(toRemove);
		}
	}
}
