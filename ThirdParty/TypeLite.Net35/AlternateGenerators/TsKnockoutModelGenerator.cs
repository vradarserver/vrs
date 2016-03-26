using System.Collections.Generic;
using TypeLite.TsModels;

namespace TypeLite.AlternateGenerators
{

    /// <summary>
    /// Generator implementation converting member types to KnockoutObservable<T> and KnockoutObservableArray<T>
    /// </summary>
    public class TsKnockoutModelGenerator : TsGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classModel"></param>
        /// <param name="sb"></param>
        /// <param name="generatorOutput"></param>
        protected override void AppendClassDefinition(TsClass classModel, ScriptBuilder sb, TsGeneratorOutput generatorOutput)
        {
            string typeName = this.GetTypeName(classModel);
            string visibility = this.GetTypeVisibility(classModel, typeName) ? "export " : "";
            sb.AppendFormatIndented("{0}interface {1}", visibility, typeName);
            if (classModel.BaseType != null)
                sb.AppendFormat(" extends {0}", this.GetFullyQualifiedTypeName(classModel.BaseType));
            sb.AppendLine(" {");
            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
                members.AddRange(classModel.Properties);
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields)
                members.AddRange(classModel.Fields);
            using (sb.IncreaseIndentation())
            {
                foreach (var property in members)
                {
                    if (property.IsIgnored)
                        continue;
                    var propTypeName = this.GetPropertyType(property);
                    if (property.PropertyType.IsCollection())
                    { 
                        //Note: new member functon checking if property is collection or not
                        //Also remove the array brackets from the name
                        if (propTypeName.Length > 2 && propTypeName.Substring(propTypeName.Length-2) == "[]")
                            propTypeName = propTypeName.Substring(0, propTypeName.Length - 2);
                        propTypeName = "KnockoutObservableArray<" + propTypeName + ">";
                    } else
                        propTypeName = "KnockoutObservable<" + propTypeName + ">";
                    sb.AppendLineIndented(string.Format("{0}: {1};", this.GetPropertyName(property), propTypeName));
                }
            }
            sb.AppendLineIndented("}");
            _generatedClasses.Add(classModel);
        }
    }
}
