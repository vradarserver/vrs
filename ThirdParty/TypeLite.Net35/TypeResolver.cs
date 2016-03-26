using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite {
    /// <summary>
    /// Resolves TsTypes to more specialized types 
    /// </summary>
    /// <remarks>
    /// When a class is added to the model by TsModelBuilder, TsType is used for all type references. The purpose of the TypeResolver is to visit references and resolve them to the specific types.
    /// </remarks>
    internal class TypeResolver : TsModelVisitor {
        TsModel _model;
        Dictionary<Type, TsType> _knownTypes;
        Dictionary<string, TsModule> _modules;

        /// <summary>
        /// Initializes a new instance of the TypeResolver.
        /// </summary>
        /// <param name="model">The model to process.</param>
        public TypeResolver(TsModel model) {
            _model = model;
            _modules = new Dictionary<string, TsModule>();
            _knownTypes = new Dictionary<Type, TsType>();

            foreach (var classModel in model.Classes) {
                _knownTypes[classModel.Type] = classModel;
            }

            foreach (var enumModel in model.Enums) {
                _knownTypes[enumModel.Type] = enumModel;
            }
        }

        /// <summary>
        /// Resolves references in the class.
        /// </summary>
        /// <param name="classModel"></param>
        public override void VisitClass(TsClass classModel, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
            if (classModel.Module != null) {
                classModel.Module = this.ResolveModule(classModel.Module.Name);
            }

            if (classModel.BaseType != null && classModel.BaseType != TsType.Any) {
                classModel.BaseType = this.ResolveType(classModel.BaseType, propertyVisibilityFormatter, false);
            }

            for (int i = 0; i < classModel.Interfaces.Count; i++) {
                classModel.Interfaces[i] = this.ResolveType(classModel.Interfaces[i], propertyVisibilityFormatter, false);
            }
        }

        /// <summary>
        /// Resolves references in the enum.
        /// </summary>
        /// <param name="enumModel"></param>
        public override void VisitEnum(TsEnum enumModel) {
            if (enumModel.Module != null) {
                enumModel.Module = this.ResolveModule(enumModel.Module.Name);
            }
        }

        /// <summary>
        /// Resolves references in the property.
        /// </summary>
        /// <param name="property"></param>
        public override void VisitProperty(TsProperty property, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
            property.PropertyType = this.ResolveType(property.PropertyType, propertyVisibilityFormatter);
            if (property.GenericArguments != null) {
                for (int i = 0; i < property.GenericArguments.Count; i++) {
                    property.GenericArguments[i] = this.ResolveType(property.GenericArguments[i], propertyVisibilityFormatter);
                }
            }
        }

        /// <summary>
        /// Resolves TsType to the more specialized type.
        /// </summary>
        /// <param name="toResolve">The type to resolve.</param>
        /// <returns></returns>
        private TsType ResolveType(TsType toResolve, TsPropertyVisibilityFormatter propertyVisibilityFormatter, bool useOpenGenericDefinition = true) {
            if (!(toResolve is TsType)) {
                return toResolve;
            }

            if (_knownTypes.ContainsKey(toResolve.Type)) {
                return _knownTypes[toResolve.Type];
            } else if (toResolve.Type.IsGenericType && useOpenGenericDefinition) {
                // We stored its open type definition instead
                TsType openType = null;
                if (_knownTypes.TryGetValue(toResolve.Type.GetGenericTypeDefinition(), out openType)) {
                    return openType;
                }
            } else if (toResolve.Type.IsGenericType) {
                var genericType = TsType.Create(toResolve.Type, propertyVisibilityFormatter);
                _knownTypes[toResolve.Type] = genericType;
                return genericType;
            }

            var typeFamily = TsType.GetTypeFamily(toResolve.Type);
            TsType type = null;

            switch (typeFamily) {
                case TsTypeFamily.System: type = new TsSystemType(toResolve.Type); break;
                case TsTypeFamily.Collection: type = this.CreateCollectionType(toResolve, propertyVisibilityFormatter); break;
                case TsTypeFamily.Enum: type = new TsEnum(toResolve.Type); break;
                default: type = TsType.Any; break;
            }

            _knownTypes[toResolve.Type] = type;
            return type;
        }

        /// <summary>
        /// Creates a TsCollection from TsType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private TsCollection CreateCollectionType(TsType type, TsPropertyVisibilityFormatter propertyVisibilityFormatter) {
            var resolved = new TsCollection(type.Type, propertyVisibilityFormatter);
            resolved.ItemsType = this.ResolveType(resolved.ItemsType, propertyVisibilityFormatter, false);
            return resolved;
        }

        /// <summary>
        /// Resolves module instance from the module name.
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <returns></returns>
        private TsModule ResolveModule(string name) {
            name = name ?? string.Empty;

            if (_modules.ContainsKey(name)) {
                return _modules[name];
            }

            var module = new TsModule(name);
            _modules[name] = module;
            _model.Modules.Add(module);
            return module;
        }
    }
}
