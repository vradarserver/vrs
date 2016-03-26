using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TypeLite.TsModels;

namespace TypeLite {
	/// <summary>
	/// Provides helper methods for generating TypeScript definition files.
	/// </summary>
	public static class TypeScript {
		/// <summary>
		/// Creates an instance of the FluentTsModelBuider for use in T4 templates.
		/// </summary>
		/// <returns>An instance of the FluentTsModelBuider</returns>
		public static TypeScriptFluent Definitions() {
			return new TypeScriptFluent();
		}

		/// <summary>
		/// Creates an instance of the FluentTsModelBuider for use in T4 templates.
		/// </summary>
		/// <param name="scriptGenerator">The script generator you want it constructed with</param>
		/// <returns>An instance of the FluentTsModelBuider</returns>
		public static TypeScriptFluent Definitions(TsGenerator scriptGenerator) {
			return new TypeScriptFluent(scriptGenerator);
		}
	}

	/// <summary>
	/// Represents a wrapper around TsModelBuilder and TsGenerator that simplify usage a enables fluent configuration.
	/// </summary>
	public class TypeScriptFluent {
		protected TsModelBuilder _modelBuilder;
		protected TsGenerator _scriptGenerator;

		/// <summary>
		/// Gets the ModelBuilder being configured with fluent configuration.
		/// </summary>
		public TsModelBuilder ModelBuilder {
			get { return _modelBuilder; }
		}

		/// <summary>
		/// Gets the ModelBuilder being configured with fluent configuration.
		/// </summary>
		public TsGenerator ScriptGenerator {
			get { return _scriptGenerator; }
		}

		/// <summary>
		/// Initializes a new instance of the TypeScriptFluent class
		/// </summary>
		public TypeScriptFluent() {
			_modelBuilder = new TsModelBuilder();
			_scriptGenerator = new TsGenerator();
		}

		/// <summary>
		/// Initializes a new instance of the TypeScriptFluent class
		/// </summary>
		public TypeScriptFluent(TsGenerator scriptGenerator) {
			_modelBuilder = new TsModelBuilder();
			_scriptGenerator = scriptGenerator;
		}

		/// <summary>
		/// Initializes a new instance of the TypeScriptFluent class
		/// </summary>
		/// <param name="fluentConfigurator">The source fluent configurator</param>
		protected TypeScriptFluent(TypeScriptFluent fluentConfigurator) {
			_modelBuilder = fluentConfigurator._modelBuilder;
			_scriptGenerator = fluentConfigurator._scriptGenerator;
		}

		/// <summary>
		/// Adds specific class with all referenced classes to the model.
		/// </summary>
		/// <typeparam name="T">The class type to add.</typeparam>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluentModuleMember For<T>() {
			return this.For(typeof (T));
		}

		/// <summary>
		/// Adds specific class with all referenced classes to the model.
		/// </summary>
		/// <param name="type">The type to add to the model.</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluentModuleMember For(Type type) {
			var model = _modelBuilder.Add(type);
			if (model is TsClass || model is TsEnum)
				return new TypeScriptFluentModuleMember(this, model);
			throw new InvalidOperationException("The type must be a class or an enum");
		}

		/// <summary>
		/// Adds all classes annotated with the TsClassAttribute from an assembly to the model.
		/// </summary>
		/// <param name="assembly">The assembly with classes to add.</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent For(Assembly assembly) {
			_modelBuilder.Add(assembly);
			return this;
		}

		/// <summary>
		/// Registers a formatter for the specific type
		/// </summary>
		/// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
			return WithTypeFormatter<TFor>(formatter);
		}

		/// <summary>
		/// Registers a formatter for the specific type
		/// </summary>
		/// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithTypeFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
			_scriptGenerator.RegisterTypeFormatter<TFor>(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for the the TsClass type.
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithFormatter(TsTypeFormatter formatter) {
			return WithTypeFormatter(formatter);
		}

		/// <summary>
		/// Registers a formatter for the the TsClass type.
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithTypeFormatter(TsTypeFormatter formatter) {
			_scriptGenerator.RegisterTypeFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for member identifiers
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithFormatter(TsMemberIdentifierFormatter formatter) {
			return WithMemberFormatter(formatter);
		}

		/// <summary>
		/// Registers a formatter for member identifiers
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithMemberFormatter(TsMemberIdentifierFormatter formatter) {
			_scriptGenerator.SetIdentifierFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for member types
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithFormatter(TsSimpleMemberTypeFormatter formatter) {
			TsMemberTypeFormatter tsMemberTypeFormatter =
				(tsProperty, memberTypeName) => {
					var asCollection = tsProperty.PropertyType as TsCollection;
					var isCollection = asCollection != null;

					return isCollection
						? formatter(memberTypeName, true, asCollection.Dimension)
						: formatter(memberTypeName, false);
				};

			_scriptGenerator.SetMemberTypeFormatter(tsMemberTypeFormatter);

			return this;
		}

		/// <summary>
		/// Registers a formatter for member types
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithMemberTypeFormatter(TsMemberTypeFormatter formatter) {
			_scriptGenerator.SetMemberTypeFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for module names
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithFormatter(TsSimplifiedModuleNameFormatter formatter) {
			_scriptGenerator.SetModuleNameFormatter(module => formatter(module.Name));
			return this;
		}

		/// <summary>
		/// Registers a formatter for module names
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithModuleNameFormatter(TsModuleNameFormatter formatter) {
			_scriptGenerator.SetModuleNameFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a formatter for type visibility
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		[Obsolete]
		public TypeScriptFluent WithVisibility(TsSimpleTypeVisibilityFormatter formatter) {
			_scriptGenerator.SetTypeVisibilityFormatter((tsClass, name) => formatter(name));
			return this;
		}

		/// <summary>
		/// Registers a formatter for type visibility
		/// </summary>
		/// <param name="formatter">The formatter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithVisibility(TsTypeVisibilityFormatter formatter) {
			_scriptGenerator.SetTypeVisibilityFormatter(formatter);
			return this;
		}

		/// <summary>
		/// Registers a converter for the specific type
		/// </summary>
		/// <typeparam name="TFor">The type to register the converter for.</typeparam>
		/// <param name="convertor">The converter to register</param>
		/// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
		public TypeScriptFluent WithConvertor<TFor>(TypeConvertor convertor) {
			_scriptGenerator.RegisterTypeConvertor<TFor>(convertor);
			return this;
		}

		/// <summary>
		/// Registers a typescript reference file
		/// </summary>
		/// <param name="reference">Name of the d.ts typescript reference file</param>
		/// <returns></returns>
		public TypeScriptFluent WithReference(string reference) {
			_scriptGenerator.AddReference(reference);
			return this;
		}

		/// <summary>
		/// Sets a string for single indentation level in the output
		/// </summary>
		/// <param name="indentationString">The string used for the single indentation level.</param>
		/// <returns></returns>
		public TypeScriptFluent WithIndentation(string indentationString) {
			_scriptGenerator.IndentationString = indentationString;
			return this;
		}

        public TypeScriptFluent WithPropertyVisibilityFormatter(TsPropertyVisibilityFormatter formatter) {
            _modelBuilder.PropertyVisibilityFormatter = formatter;
            return this;
        }

        /// <summary>
        /// Sets format of generated enums.
        /// </summary>
        /// <param name="value">Boolean value indicating whether the enums should be generated as 'const enum'</param>
        /// <returns>Instance of the TypeScriptFluent that enables fluent configuration.</returns>
        public TypeScriptFluent AsConstEnums(bool value = true) {
            _scriptGenerator.GenerateConstEnums = value;
            return this;
        }

		/// <summary>
		/// Generates TypeScript definitions for types included in this model builder.
		/// </summary>
		/// <returns>TypeScript definition for types included in this model builder.</returns>
		public string Generate() {
			var model = _modelBuilder.Build();
			return _scriptGenerator.Generate(model);
		}

		/// <summary>
		/// Generates TypeScript definitions for types included in this model builder. Optionally restricts output to classes or enums.
		/// </summary>
		/// <param name="output">The type of definitions to generate</param>
		/// <returns>TypeScript definition for types included in this model builder.</returns>
		public string Generate(TsGeneratorOutput output) {
			var model = _modelBuilder.Build();
			return _scriptGenerator.Generate(model, output);
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>TypeScript definition for types included in this model builder.</returns>
		public override string ToString() {
			return this.Generate();
		}
	}

	/// <summary>
	/// Represents a wrapper around TsModelBuilder and TsGenerator that simplify usage a enables fluent configuration for types.
	/// </summary>
	public class TypeScriptFluentModuleMember : TypeScriptFluent {
		/// <summary>
		/// Gets the class being configured.
		/// </summary>
		public TsModuleMember Member { get; protected set; }

		internal TypeScriptFluentModuleMember(TypeScriptFluent fluentConfigurator, TsModuleMember member)
			: base(fluentConfigurator) {
			this.Member = member;
		}

		/// <summary>
		/// Changes the name of the type being configured .
		/// </summary>
		/// <param name="name">The new name of the type</param>
		/// <returns>Instance of the TypeScriptFluentModuleMember that enables fluent configuration.</returns>
		public TypeScriptFluentModuleMember Named(string name) {
			this.Member.Name = name;
			return this;
		}

		/// <summary>
		/// Maps the type being configured to the specific module
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <returns>Instance of the TypeScriptFluentModuleMember that enables fluent configuration.</returns>
		public TypeScriptFluentModuleMember ToModule(string moduleName) {
			this.Member.Module = new TsModule(moduleName);
			return this;
		}

		/// <summary>
		/// Ignores this member when generating typescript
		/// </summary>
		/// <returns>Instance of the TypeScriptFluentModuleMember that enables fluent configuration.</returns>
		public TypeScriptFluentModuleMember Ignore()
		{
			if (Member is TsClass)
			{
				((TsClass)Member).IsIgnored = true;
			}
			return this;
		}
	}
}