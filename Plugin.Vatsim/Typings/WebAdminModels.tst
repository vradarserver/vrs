// More info: http://frhagn.github.io/Typewriter/

${
    using Typewriter.Extensions.Types;

    string WebAdminType(Property prop)
    {
        var result = prop.Type.ToString();
        if(!prop.Type.IsPrimitive) {
            if(prop.Type.FullName == "VirtualRadar.Interface.View.ValidationModelField") {
                result = "VirtualRadar.Interface.View.IValidationModelField";
            } else {
                result = $"I{prop.Type.Name}";
            }
        }

        return result;
    }

    string WebAdminKOType(Property prop)
    {
        var result = $"KnockoutObservable<{prop.Type.Name}>";

        if(!prop.Type.IsPrimitive) {
            if(prop.Type.FullName == "VirtualRadar.Interface.View.ValidationModelField") {
                result = "VirtualRadar.Interface.View.IValidationModelField_KO";
            } else {
                if(!prop.Type.IsEnumerable) {
                    result = $"I{prop.Type.Name}_KO";
                } else {
                    result = $"KnockoutViewModelArray<I{prop.Type.Name.Substring(0, prop.Type.Name.Length - 2)}_KO>";
                }
            }
        }

        return result;
    }
}

/// <reference path="../../plugin.webadmin/typings/knockout.d.ts" />
/// <reference path="../../plugin.webadmin/typings/knockout.viewmodel.d.ts" />

declare module VirtualRadar.Plugin.Vatsim.WebAdmin {
$Classes(VirtualRadar.Plugin.Vatsim.WebAdmin.*Model)[
    interface I$Name {$Properties[
        $Name: $WebAdminType;]
    }
]
}

declare module VirtualRadar.Plugin.Vatsim.WebAdmin {
$Classes(VirtualRadar.Plugin.Vatsim.WebAdmin.*Model)[
    interface I$Name_KO {$Properties[
        $Name: $WebAdminKOType;]
    }
]
}