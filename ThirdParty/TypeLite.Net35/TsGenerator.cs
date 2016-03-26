using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TypeLite.Extensions;
using TypeLite.ReadOnlyDictionary;
using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// Generates TypeScript definitions form the code model.
    /// </summary>
    public class TsGenerator {
        protected TsTypeFormatterCollection _typeFormatters;
        protected TypeConvertorCollection _typeConvertors;
        protected TsMemberIdentifierFormatter _memberFormatter;
        protected TsMemberTypeFormatter _memberTypeFormatter;
        protected TsTypeVisibilityFormatter _typeVisibilityFormatter;
        protected TsModuleNameFormatter _moduleNameFormatter;
        protected IDocAppender _docAppender;
        protected HashSet<TsClass> _generatedClasses;
        protected HashSet<TsEnum> _generatedEnums;
        protected List<string> _references;

        /// <summary>
        /// Gets collection of formatters for individual TsTypes
        /// </summary>
        public IReadOnlyDictionary<Type, TsTypeFormatter> Formaters {
            get {
                return new ReadOnlyDictionaryWrapper<Type, TsTypeFormatter>(_typeFormatters._formatters);
            }
        }

        /// <summary>
        /// Gets or sets string for the single indentation level.
        /// </summary>
        public string IndentationString { get; set; }

        /// <summary>
        /// Gets or sets bool value indicating whether enums should be generated as 'const enum'. Default value is true.
        /// </summary>
        public bool GenerateConstEnums { get; set; }

        /// <summary>
        /// Initializes a new instance of the TsGenerator class with the default formatters.
        /// </summary>
        public TsGenerator() {
            _references = new List<string>();
            _generatedClasses = new HashSet<TsClass>();
            _generatedEnums = new HashSet<TsEnum>();

            _typeFormatters = new TsTypeFormatterCollection();
            _typeFormatters.RegisterTypeFormatter<TsClass>((type, formatter) => {
                var tsClass = ((TsClass)type);
                if (!tsClass.GenericArguments.Any()) return tsClass.Name;
                return tsClass.Name + "<" + string.Join(", ", tsClass.GenericArguments.Select(a => a as TsCollection != null ? this.GetFullyQualifiedTypeName(a) + "[]" : this.GetFullyQualifiedTypeName(a)).ToArray()) + ">";
            });
            _typeFormatters.RegisterTypeFormatter<TsSystemType>((type, formatter) => ((TsSystemType)type).Kind.ToTypeScriptString());
            _typeFormatters.RegisterTypeFormatter<TsCollection>((type, formatter) => {
                var itemType = ((TsCollection)type).ItemsType;
                var itemTypeAsClass = itemType as TsClass;
                if (itemTypeAsClass == null || !itemTypeAsClass.GenericArguments.Any()) return this.GetTypeName(itemType);
                return this.GetTypeName(itemType);
            });
            _typeFormatters.RegisterTypeFormatter<TsEnum>((type, formatter) => ((TsEnum)type).Name);

            _typeConvertors = new TypeConvertorCollection();

            _docAppender = new NullDocAppender();

            _memberFormatter = DefaultMemberFormatter;
            _memberTypeFormatter = DefaultMemberTypeFormatter;
            _typeVisibilityFormatter = DefaultTypeVisibilityFormatter;
            _moduleNameFormatter = DefaultModuleNameFormatter;

            this.IndentationString = "    ";
            this.GenerateConstEnums = true;
        }

        public bool DefaultTypeVisibilityFormatter(TsClass tsClass, string typeName) {
            return false;
        }

        public string DefaultModuleNameFormatter(TsModule module) {
            return module.Name;
        }

        public string DefaultMemberFormatter(TsProperty identifier) {
            return identifier.Name;
        }

        public string DefaultMemberTypeFormatter(TsProperty tsProperty, string memberTypeName) {
            var asCollection = tsProperty.PropertyType as TsCollection;
            var isCollection = asCollection != null;

            var result = new StringBuilder(memberTypeName);
            if(isCollection) {
                for(var i = 0;i < asCollection.Dimension;++i) {
                    result.Append("[]");
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Registers the formatter for the specific TsType
        /// </summary>
        /// <typeparam name="TFor">The type to register the formatter for. TFor is restricted to TsType and derived classes.</typeparam>
        /// <param name="formatter">The formatter to register</param>
        /// <remarks>
        /// If a formatter for the type is already registered, it is overwritten with the new value.
        /// </remarks>
        public void RegisterTypeFormatter<TFor>(TsTypeFormatter formatter) where TFor : TsType {
            _typeFormatters.RegisterTypeFormatter<TFor>(formatter);
        }

        /// <summary>
        /// Registers the custom formatter for the TsClass type.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void RegisterTypeFormatter(TsTypeFormatter formatter) {
            _typeFormatters.RegisterTypeFormatter<TsClass>(formatter);
        }

        /// <summary>
        /// Registers the converter for the specific Type
        /// </summary>
        /// <typeparam name="TFor">The type to register the converter for.</typeparam>
        /// <param name="convertor">The converter to register</param>
        /// <remarks>
        /// If a converter for the type is already registered, it is overwritten with the new value.
        /// </remarks>
        public void RegisterTypeConvertor<TFor>(TypeConvertor convertor) {
            _typeConvertors.RegisterTypeConverter<TFor>(convertor);
        }

        /// <summary>
        /// Sets the formatter for class member identifiers.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void SetIdentifierFormatter(TsMemberIdentifierFormatter formatter) {
            _memberFormatter = formatter;
        }

        /// <summary>
        /// Sets the formatter for class member types.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void SetMemberTypeFormatter(TsMemberTypeFormatter formatter) {
            _memberTypeFormatter = formatter;
        }

        /// <summary>
        /// Sets the formatter for class member types.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void SetTypeVisibilityFormatter(TsTypeVisibilityFormatter formatter) {
            _typeVisibilityFormatter = formatter;
        }

        /// <summary>
        /// Sets the formatter for module names.
        /// </summary>
        /// <param name="formatter">The formatter to register.</param>
        public void SetModuleNameFormatter(TsModuleNameFormatter formatter) {
            _moduleNameFormatter = formatter;
        }

        /// <summary>
        /// Sets the document appender.
        /// </summary>
        /// <param name="appender">The ducument appender.</param>
        public void SetDocAppender(IDocAppender appender) {
            _docAppender = appender;
        }

        /// <summary>
        /// Add a typescript reference
        /// </summary>
        /// <param name="reference">Name of d.ts file used as typescript reference</param>
        public void AddReference(string reference) {
            _references.Add(reference);
        }

        /// <summary>
        /// Generates TypeScript definitions for properties and enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <returns>TypeScript definitions for classes in the model.</returns>
        public string Generate(TsModel model) {
            return this.Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Enums);
        }

        /// <summary>
        /// Generates TypeScript definitions for classes and/or enums in the model.
        /// </summary>
        /// <param name="model">The code model with classes to generate definitions for.</param>
        /// <param name="generatorOutput">The type of definitions to generate</param>
        /// <returns>TypeScript definitions for classes and/or enums in the model..</returns>
        public string Generate(TsModel model, TsGeneratorOutput generatorOutput) {
            var sb = new ScriptBuilder(this.IndentationString);

            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties
                || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {

                if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants) {
                    // We can't generate constants together with properties or fields, because we can't set values in a .d.ts file.
                    throw new InvalidOperationException("Cannot generate constants together with properties or fields");
                }

                foreach (var reference in _references.Concat(model.References)) {
                    this.AppendReference(reference, sb);
                }
                sb.AppendLine();
            }

            foreach (var module in model.Modules) {
                this.AppendModule(module, sb, generatorOutput);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates reference to other d.ts file and appends it to the output.
        /// </summary>
        /// <param name="reference">The reference file to generate reference for.</param>
        /// <param name="sb">The output</param>
        protected virtual void AppendReference(string reference, ScriptBuilder sb) {
            sb.AppendFormat("/// <reference path=\"{0}\" />", reference);
            sb.AppendLine();
        }

        protected virtual void AppendModule(TsModule module, ScriptBuilder sb, TsGeneratorOutput generatorOutput) {
            var classes = module.Classes.Where(c => !_typeConvertors.IsConvertorRegistered(c.Type) && !c.IsIgnored).ToList();
            var enums = module.Enums.Where(e => !_typeConvertors.IsConvertorRegistered(e.Type) && !e.IsIgnored).ToList();
            if ((generatorOutput == TsGeneratorOutput.Enums && enums.Count == 0) ||
                (generatorOutput == TsGeneratorOutput.Properties && classes.Count == 0) ||
                (enums.Count == 0 && classes.Count == 0)) {
                return;
            }

            var moduleName = GetModuleName(module);
            var generateModuleHeader = moduleName != string.Empty;

            if (generateModuleHeader) {
                if (generatorOutput != TsGeneratorOutput.Enums &&
                    (generatorOutput & TsGeneratorOutput.Constants) != TsGeneratorOutput.Constants) {
                    sb.Append("declare ");
                }

                sb.AppendLine(string.Format("module {0} {{", moduleName));
            }

            using (sb.IncreaseIndentation()) {
                if ((generatorOutput & TsGeneratorOutput.Enums) == TsGeneratorOutput.Enums) {
                    foreach (var enumModel in enums) {
                        this.AppendEnumDefinition(enumModel, sb, generatorOutput);
                    }
                }

                if (((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
                    || (generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {
                    foreach (var classModel in classes) {

                        this.AppendClassDefinition(classModel, sb, generatorOutput);
                    }
                }

                if ((generatorOutput & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants) {
                    foreach (var classModel in classes) {
                        if (classModel.IsIgnored) {
                            continue;
                        }

                        this.AppendConstantModule(classModel, sb);
                    }
                }
            }
            if (generateModuleHeader) {
                sb.AppendLine("}");
            }
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        protected virtual void AppendClassDefinition(TsClass classModel, ScriptBuilder sb, TsGeneratorOutput generatorOutput) {
            string typeName = this.GetTypeName(classModel);
            string visibility = this.GetTypeVisibility(classModel, typeName) ? "export " : "";
            _docAppender.AppendClassDoc(sb, classModel, typeName);
            sb.AppendFormatIndented("{0}interface {1}", visibility, typeName);
            if (classModel.BaseType != null) {
                sb.AppendFormat(" extends {0}", this.GetFullyQualifiedTypeName(classModel.BaseType));
            }

            if (classModel.Interfaces.Count > 0) {
                var implementations = classModel.Interfaces.Select(GetFullyQualifiedTypeName).ToArray();

                var prefixFormat = classModel.Type.IsInterface ? " extends {0}"
                    : classModel.BaseType != null ? " ,"
                    : " extends {0}";

                sb.AppendFormat(prefixFormat, string.Join(" ,", implementations));
            }

            sb.AppendLine(" {");

            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties) {
                members.AddRange(classModel.Properties);
            }
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields) {
                members.AddRange(classModel.Fields);
            }
            using (sb.IncreaseIndentation()) {
                foreach (var property in members) {
                    if (property.IsIgnored) {
                        continue;
                    }

                    _docAppender.AppendPropertyDoc(sb, property, this.GetPropertyName(property), this.GetPropertyType(property));
                    sb.AppendLineIndented(string.Format("{0}: {1};", this.GetPropertyName(property), this.GetPropertyType(property)));
                }
            }

            sb.AppendLineIndented("}");

            _generatedClasses.Add(classModel);
        }

        protected virtual void AppendEnumDefinition(TsEnum enumModel, ScriptBuilder sb, TsGeneratorOutput output) {
            string typeName = this.GetTypeName(enumModel);
            string visibility = (output & TsGeneratorOutput.Enums) == TsGeneratorOutput.Enums || (output & TsGeneratorOutput.Constants) == TsGeneratorOutput.Constants ? "export " : "";

            _docAppender.AppendEnumDoc(sb, enumModel, typeName);

            string constSpecifier = this.GenerateConstEnums ? "const " : string.Empty;
            sb.AppendLineIndented(string.Format("{0}{2}enum {1} {{", visibility, typeName, constSpecifier));

            using (sb.IncreaseIndentation()) {
                int i = 1;
                foreach (var v in enumModel.Values) {
                    _docAppender.AppendEnumValueDoc(sb, v);
                    sb.AppendLineIndented(string.Format(i < enumModel.Values.Count ? "{0} = {1}," : "{0} = {1}", v.Name, v.Value));
                    i++;
                }
            }

            sb.AppendLineIndented("}");

            _generatedEnums.Add(enumModel);
        }

        /// <summary>
        /// Generates class definition and appends it to the output.
        /// </summary>
        /// <param name="classModel">The class to generate definition for.</param>
        /// <param name="sb">The output.</param>
        /// <param name="generatorOutput"></param>
        protected virtual void AppendConstantModule(TsClass classModel, ScriptBuilder sb) {
            if (!classModel.Constants.Any()) {
                return;
            }

            string typeName = this.GetTypeName(classModel);
            sb.AppendLineIndented(string.Format("export module {0} {{", typeName));

            using (sb.IncreaseIndentation()) {
                foreach (var property in classModel.Constants) {
                    if (property.IsIgnored) {
                        continue;
                    }

                    _docAppender.AppendConstantModuleDoc(sb, property, this.GetPropertyName(property), this.GetPropertyType(property));
                    sb.AppendFormatIndented("export var {0}: {1} = {2};", this.GetPropertyName(property), this.GetPropertyType(property), this.GetPropertyConstantValue(property));
                    sb.AppendLine();
                }

            }
            sb.AppendLineIndented("}");

            _generatedClasses.Add(classModel);
        }

        /// <summary>
        /// Gets fully qualified name of the type
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>Fully qualified name of the type</returns>
        public string GetFullyQualifiedTypeName(TsType type) {
            var moduleName = string.Empty;

            if (type as TsModuleMember != null && !_typeConvertors.IsConvertorRegistered(type.Type)) {
                var memberType = (TsModuleMember)type;
                moduleName = memberType.Module != null ? GetModuleName(memberType.Module) : string.Empty;
            } else if (type as TsCollection != null) {
                var collectionType = (TsCollection)type;
                moduleName = GetCollectionModuleName(collectionType, moduleName);
            }

            if (type.Type.IsGenericParameter) {
                return this.GetTypeName(type);
            }
            if (!string.IsNullOrEmpty(moduleName)) {
                var name = moduleName + "." + this.GetTypeName(type);
                return name;
            }

            return this.GetTypeName(type);
        }

        /// <summary>
        /// Recursively finds the module name for the underlaying ItemsType of a TsCollection.
        /// </summary>
        /// <param name="collectionType">The TsCollection object.</param>
        /// <param name="moduleName">The module name.</param>
        /// <returns></returns>
        public string GetCollectionModuleName(TsCollection collectionType, string moduleName) {
            if (collectionType.ItemsType as TsModuleMember != null && !_typeConvertors.IsConvertorRegistered(collectionType.ItemsType.Type)) {
                if (!collectionType.ItemsType.Type.IsGenericParameter)
                    moduleName = ((TsModuleMember)collectionType.ItemsType).Module != null ? GetModuleName(((TsModuleMember)collectionType.ItemsType).Module) : string.Empty;
            }
            if (collectionType.ItemsType as TsCollection != null) {
                moduleName = GetCollectionModuleName((TsCollection)collectionType.ItemsType, moduleName);
            }
            return moduleName;
        }

        /// <summary>
        /// Gets name of the type in the TypeScript
        /// </summary>
        /// <param name="type">The type to get name of</param>
        /// <returns>name of the type</returns>
        public string GetTypeName(TsType type) {
            if (_typeConvertors.IsConvertorRegistered(type.Type)) {
                return _typeConvertors.ConvertType(type.Type);
            }

            return _typeFormatters.FormatType(type);
        }

        /// <summary>
        /// Gets property name in the TypeScript
        /// </summary>
        /// <param name="property">The property to get name of</param>
        /// <returns>name of the property</returns>
        public string GetPropertyName(TsProperty property) {
            var name = _memberFormatter(property);
            if (property.IsOptional) {
                name += "?";
            }

            return name;
        }

        /// <summary>
        /// Gets property type in the TypeScript
        /// </summary>
        /// <param name="property">The property to get type of</param>
        /// <returns>type of the property</returns>
        public string GetPropertyType(TsProperty property) {
            var fullyQualifiedTypeName = GetFullyQualifiedTypeName(property.PropertyType);
            return _memberTypeFormatter(property, fullyQualifiedTypeName);
        }

        /// <summary>
        /// Gets property constant value in TypeScript format
        /// </summary>
        /// <param name="property">The property to get constant value of</param>
        /// <returns>constant value of the property</returns>
        public string GetPropertyConstantValue(TsProperty property) {
            var quote = property.PropertyType.Type == typeof(string) ? "\"" : "";
            return quote + property.ConstantValue.ToString() + quote;
        }

        /// <summary>
        /// Gets whether a type should be marked with "Export" keyword in TypeScript
        /// </summary>
        /// <param name="tsClass"></param>
        /// <param name="typeName">The type to get the visibility of</param>
        /// <returns>bool indicating if type should be marked weith keyword "Export"</returns>
        public bool GetTypeVisibility(TsClass tsClass, string typeName) {
            return _typeVisibilityFormatter(tsClass, typeName);
        }

        /// <summary>
        /// Formats a module name
        /// </summary>
        /// <param name="module">The module to be formatted</param>
        /// <returns>The module name after formatting.</returns>
        public string GetModuleName(TsModule module) {
            return _moduleNameFormatter(module);
        }

    }
}
