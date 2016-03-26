using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeLite.TsModels;

namespace TypeLite.AlternateGenerators
{
    /// <summary>
    /// Generator implementation that emits classes which extend Backbone.Model
    /// </summary>
    public class TsBackboneModelGenerator : TsGenerator
    {

        protected override void AppendClassDefinition(TsClass classModel, ScriptBuilder sb, TsGeneratorOutput generatorOutput)
        {

            string typeName = this.GetTypeName(classModel);
            string visibility = this.GetTypeVisibility(classModel, typeName) ? "export " : "";
            sb.AppendFormatIndented(
                "{0}class {1} extends {2}", 
                visibility, 
                typeName,
                //all bottom-level classes must extend Backbone.Model.
                classModel.BaseType != null ? this.GetFullyQualifiedTypeName(classModel.BaseType) : "Backbone.Model");

            sb.AppendLine(" {");

            var members = new List<TsProperty>();
            if ((generatorOutput & TsGeneratorOutput.Properties) == TsGeneratorOutput.Properties)
            {
                members.AddRange(classModel.Properties);
            }
            if ((generatorOutput & TsGeneratorOutput.Fields) == TsGeneratorOutput.Fields)
            {
                members.AddRange(classModel.Fields);
            }
            using (sb.IncreaseIndentation())
            {
                foreach (var property in members)
                {
                    if (property.IsIgnored)
                    {
                        continue;
                    }

                    sb.AppendLineIndented(string.Format(
                        "get {0}(): {1} {{ return this.get(\"{0}\"); }}",
                        this.GetPropertyName(property), this.GetPropertyType(property)));

                    sb.AppendLineIndented(string.Format(
                        "set {0}(v: {1}) {{ this.set(\"{0}\", v); }}",
                        this.GetPropertyName(property), this.GetPropertyType(property)));
                }
            }

            sb.AppendLineIndented("}");

            _generatedClasses.Add(classModel);
        }
    }
}
