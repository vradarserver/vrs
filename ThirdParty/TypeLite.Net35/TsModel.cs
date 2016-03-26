using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Represents script model of CLR classes.
	/// </summary>
	public class TsModel {
		/// <summary>
		/// Gets a collection of classes in the model.
		/// </summary>
		public HashSet<TsClass> Classes { get; private set; }

		/// <summary>
		/// Gets a collection of enums in the model
		/// </summary>
		public HashSet<TsEnum> Enums { get; private set; }

		/// <summary>
		/// Gets a collection of references to other d.ts files.
		/// </summary>
		public HashSet<string> References { get; private set; }

		/// <summary>
		/// Gets a collection of modules in the module.
		/// </summary>
		public HashSet<TsModule> Modules { get; private set; }

		/// <summary>
		/// Initializes a new instance of the TsModel class.
		/// </summary>
		public TsModel()
			: this(new TsClass[] { }) {
		}

		/// <summary>
		/// Initializes a new instance of the TsModel class with collection of classes.
		/// </summary>
		/// <param name="classes">The collection of classes to add to the model.</param>
		public TsModel(IEnumerable<TsClass> classes) {
			this.Classes = new HashSet<TsClass>(classes);
			this.References = new HashSet<string>();
			this.Modules = new HashSet<TsModule>();
			this.Enums = new HashSet<TsEnum>();
		}

		/// <summary>
		/// Initializes a new instance of the TsModel class with collection of classes and enums
		/// </summary>
		/// <param name="classes">The collection of classes to add to the model.</param>
		/// <param name="enums">The collection of enums to add to the model.</param>
		public TsModel(IEnumerable<TsClass> classes, IEnumerable<TsEnum> enums) {
			this.Classes = new HashSet<TsClass>(classes);
			this.References = new HashSet<string>();
			this.Modules = new HashSet<TsModule>();
			this.Enums = new HashSet<TsEnum>(enums);
		}

		/// <summary>
		/// Runs specific model visitor.
		/// </summary>
		/// <param name="visitor">The model visitor to run.</param>
		public void RunVisitor(ITsModelVisitor visitor, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
			visitor.VisitModel(this);

			foreach (var module in this.Modules) {
				visitor.VisitModule(module);
			}

			foreach (var classModel in this.Classes) {
				visitor.VisitClass(classModel, propertyVisibilityFormatter);

				foreach (var property in classModel.Properties.Union(classModel.Fields).Union(classModel.Constants)) {
					visitor.VisitProperty(property, propertyVisibilityFormatter);
				}
			}

			foreach (var enumModel in this.Enums) {
				visitor.VisitEnum(enumModel);
			}
		}
	}
}
