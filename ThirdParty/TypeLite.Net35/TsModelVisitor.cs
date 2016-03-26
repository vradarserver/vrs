using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Provides base class for model visitor.
	/// </summary>
	public abstract class TsModelVisitor : ITsModelVisitor {
		/// <summary>
		/// When overridden in a derived class, it can examine or modify the whole model.
		/// </summary>
		/// <param name="model">The code model being visited.</param>
		public virtual void VisitModel(TsModel model) {
		}

		/// <summary>
		/// When overridden in a derived class, it can examine or modify the script module.
		/// </summary>
		/// <param name="module">The module being visited.</param>
		public virtual void VisitModule(TsModule module) {
		}

		/// <summary>
		/// When overridden in a derived class, it can examine or modify the class model.
		/// </summary>
		/// <param name="classModel">The model class being visited.</param>
		public virtual void VisitClass(TsClass classModel, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
		}

		/// <summary>
		/// When overridden in a derived class, it can examine or modify the property model.
		/// </summary>
		/// <param name="property">The model property being visited.</param>
		public virtual void VisitProperty(TsProperty property, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
		}

		/// <summary>
		/// When overridden in a derived class, it can examine or modify the enum model.
		/// </summary>
		/// <param name="enumModel">The model enum being visited.</param>
		public virtual void VisitEnum(TsEnum enumModel) {
		}
	}
}
