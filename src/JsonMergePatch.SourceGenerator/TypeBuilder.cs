using System.Reflection;
using LaDeak.JsonMergePatch.SourceGenerator.ApplyPatchBuilders;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator;

public record PropertyInfo
{
    public PropertyInfo(IPropertySymbol symbol)
    {
        Name = symbol.Name;
        Attributes = ReadAttributes(symbol).ToList();
        IsInitOnly = GetIsInitOnly(symbol);
        HasGeneratableType = GeneratedTypeFilter.IsGeneratableType(symbol.Type);
        PropertyTypeName = GetPropertyTypeName(symbol);
    }

    private static bool GetIsInitOnly(IPropertySymbol symbol)
    {
        return symbol.SetMethod?.OriginalDefinition.IsInitOnly ?? false;
    }

    public string Name { get; }

    public List<AttributeData> Attributes { get; }

    public bool HasGeneratableType { get; }

    public bool IsInitOnly { get; }

    public string PropertyTypeName { get; }

    public ApplyPatchBuilder? Builder { get; private set; }

    private IEnumerable<AttributeData> ReadAttributes(IPropertySymbol symbol)
    {
        return symbol.GetAttributes().Where(attribute =>
            attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableContextAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "LaDeak.JsonMergePatch.Abstractions.PatchableAttribute");
    }

    private string GetPropertyTypeName(IPropertySymbol symbol)
    {
        if (symbol.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
            return CreateGenericTypeWithParameters(symbol);

        return ConvertToNullableIfRequired(symbol.Type).Item1;
    }

    private string CreateGenericTypeWithParameters(IPropertySymbol propertySymbol)
    {
        if (propertySymbol?.Type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
            throw new InvalidOperationException("Parameter is not generic type parameter.");

        Builder = new SimpleNonGeneratableBuilder(Name, false);
        var firstUnderlyingType = GetPropertyTypeName(namedType.TypeArguments.First()).TypeName;
        var withoutUnderlyingType = namedType.ToDisplayString(new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeType, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.IncludeType, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.ExpandNullable));

        var genericResult = $"{withoutUnderlyingType}<{firstUnderlyingType}";
        bool isConvertedToNullableType = false;
        foreach (var underlyingType in namedType.TypeArguments.Skip(1).OfType<INamedTypeSymbol>())
        {
            (string genericTypeParam, bool isConvertedToNullable) = ConvertToNullableIfRequired(underlyingType);
            isConvertedToNullableType |= isConvertedToNullable;
            genericResult += $", {genericTypeParam}";
        }
        genericResult += ">";
        if (namedType.SpecialType != SpecialType.System_Nullable_T && namedType.NullableAnnotation != NullableAnnotation.Annotated)
        {
            genericResult += "?";
        }
        if (namedType.Name.Contains("Dictionary") && namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic")
            Builder = new NonGeneratableDictionaryPatchBuilder(namedType, Name, HasGeneratableType, isConvertedToNullableType);

        if (namedType.Name.Contains("List")
            && namedType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
            && !GetIsInitOnly(propertySymbol)
            && GeneratedTypeFilter.IsGeneratableType(namedType.TypeArguments.First()))
            Builder = new GeneratableListBuilder(namedType, Name);

        return genericResult;
    }

    private (string, bool) ConvertToNullableIfRequired(ITypeSymbol typeSymbol)
    {
        (bool isGeneratable, string genericTypeParam) = GetPropertyTypeName(typeSymbol);
        bool isConvertedToNullableType = false;
        if (typeSymbol.IsValueType && typeSymbol.SpecialType != SpecialType.System_Nullable_T && typeSymbol.NullableAnnotation != NullableAnnotation.Annotated)
        {
            isConvertedToNullableType = true;
            genericTypeParam = $"System.Nullable<{genericTypeParam}>";
        }
        if (!typeSymbol.IsValueType && typeSymbol.SpecialType != SpecialType.System_Nullable_T)
        {
            genericTypeParam = $"{genericTypeParam}?";
        }

        if (isGeneratable)
            Builder = new SimpleGeneratableBuilder(Name);
        else
            Builder = new SimpleNonGeneratableBuilder(Name, isConvertedToNullableType);

        return (genericTypeParam, isConvertedToNullableType);
    }

    private (bool IsGeneratedType, string TypeName) GetPropertyTypeName(ITypeSymbol propertyTypeSymbol)
    {
        if (GeneratedTypeFilter.IsGeneratableType(propertyTypeSymbol))
        {
            return (true, NameBuilder.GetFullTypeName(propertyTypeSymbol));
        }
        return (false, propertyTypeSymbol.ToDisplayString(GeneratedTypeFilter.SymbolFormat));
    }
}

public record GeneratorClassInfo
{
    public static GeneratorClassInfo Default => new GeneratorClassInfo();

    private GeneratorClassInfo()
    {
        Name = string.Empty;
        FullTypeName = string.Empty;
        NamespaceExtension = string.Empty;
        SourceTypeName = string.Empty;
        Namespace = string.Empty;
        AssemblyName = string.Empty;
        Properties = [];
        Attributes = [];
    }

    public GeneratorClassInfo(ITypeSymbol typeSymbol, string sourceTypeName, string assemblyName = "")
    {
        Name = NameBuilder.GetName(typeSymbol);
        FullTypeName = NameBuilder.GetFullTypeName(typeSymbol);
        NamespaceExtension = NameBuilder.GetNamespaceExtension(typeSymbol);
        HasDefaultCtor = HasDefaultConstructor(typeSymbol);
        SourceTypeName = sourceTypeName;
        AssemblyName = assemblyName;
        Properties = ReadProperties(typeSymbol);
        Attributes = ReadAttributes(typeSymbol).ToList();
        Namespace = NameBuilder.GetNamespace(typeSymbol);
        CtorParameterLength = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Constructor)
            .OrderByDescending(x => x.Parameters.Length).FirstOrDefault()?.Parameters.Count() ?? 0;
    }

    public string Name { get; }

    public string FullTypeName { get; }

    public string NamespaceExtension { get; }

    public string Namespace { get; }

    public bool HasDefaultCtor { get; }

    public string SourceTypeName { get; }
    public string AssemblyName { get; }
    public int CtorParameterLength { get; }

    public List<PropertyInfo> Properties { get; }

    public List<AttributeData> Attributes { get; }

    private bool HasDefaultConstructor(ITypeSymbol typeSymbol) =>
        typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Constructor).AnyOrNull(x => x.Parameters.IsEmpty);

    private List<PropertyInfo> ReadProperties(ITypeSymbol typeSymbol)
    {
        var properties = new List<PropertyInfo>();
        var currentType = typeSymbol;
        while (currentType != null && currentType.Name != "Object")
        {
            properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>()
                       .Where(x => !x.IsReadOnly && !x.IsWriteOnly && !x.IsIndexer && !x.IsStatic && !x.IsAbstract && !x.IsVirtual)
                       .Select(x => new PropertyInfo(x)));
            currentType = currentType.BaseType;
        }
        return properties;
    }

    private IEnumerable<AttributeData> ReadAttributes(ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Where(attribute =>
            attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableContextAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "LaDeak.JsonMergePatch.Abstractions.PatchableAttribute");
    }
}

public class TypeBuilder : ITypeBuilder
{
    public GeneratedWrapper BuildWrapperType(GeneratorClassInfo typeInfo)
    {
        var name = typeInfo.Name;
        var state = new BuilderState(typeInfo);
        BuildFile(state);
        return new GeneratedWrapper()
        {
            FileName = $"LaDeakJsonMergePatch{typeInfo.NamespaceExtension}{name}",
            SourceCode = state.Builder.ToString(),
        };
    }

    private void BuildFile(BuilderState state) => BuildNamespace(state, BuildClass);

    private void BuildClass(BuilderState state) => BuildClassDeclaration(state, s => BuildClassBody(s));

    private void BuildNamespace(BuilderState state, Action<BuilderState>? addBody = null)
    {
        state.AppendLine($"#nullable enable");
        state.AppendLine($"namespace {state.TypeInfo.Namespace}");
        state.AppendLine("{");
        addBody?.Invoke(state.IncrementIdentation());
        state.AppendLine("}");
        state.AppendLine($"#nullable disable");
    }

    private void BuildClassDeclaration(BuilderState state, Action<BuilderState>? addBody = null)
    {
        BuildAttributes(state, state.TypeInfo.Attributes);
        state.AppendLine($"public class {state.TypeInfo.Name} : LaDeak.JsonMergePatch.Abstractions.Patch<{state.TypeInfo.SourceTypeName}>");
        state.AppendLine("{");
        addBody?.Invoke(state.IncrementIdentation());
        state.AppendLine("}");
    }

    private void BuildConstructor(BuilderState state, Action<BuilderState>? addBody = null)
    {
        state.AppendLine($"public {state.TypeInfo.Name}()");
        state.AppendLine("{");
        var innerState = state.IncrementIdentation();
        innerState.AppendLine($"Properties = new bool[{state.TypeInfo.Properties.Count}];");
        addBody?.Invoke(innerState);
        state.AppendLine("}");
    }

    private void BuildPropery(BuilderState state, PropertyInfo propertyInfo, int propertyId)
    {
        string fieldName = Casing.PrefixUnderscoreCamelCase(propertyInfo.Name);
        string propertyTypeName = propertyInfo.PropertyTypeName;
        state.AppendLine($"private {propertyTypeName} {fieldName};");
        BuildAttributes(state, propertyInfo.Attributes);
        state.AppendLine($"public {propertyTypeName} {propertyInfo.Name}");
        state.AppendLine("{");
        var getterSetter = state.IncrementIdentation();
        getterSetter.AppendLine($"get {{ return {fieldName}; }}");
        getterSetter.AppendLine("set");
        getterSetter.AppendLine("{");
        var setterBody = getterSetter.IncrementIdentation();
        setterBody.AppendLine($"Properties[{propertyId}] = true;");
        setterBody.AppendLine($"{fieldName} = value;");
        getterSetter.AppendLine("}");
        state.AppendLine("}");
    }

    private void BuildAttributes(BuilderState state, IEnumerable<AttributeData> attributes)
    {
        foreach (var attribute in attributes)
            if (attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableContextAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.NullableAttribute"
                && attribute.AttributeClass?.ToDisplayString() != "LaDeak.JsonMergePatch.Abstractions.PatchableAttribute")
                BuildAttribute(state, attribute);
    }

    private void BuildAttribute(BuilderState state, AttributeData attribute) => state.AppendLine($"[{attribute}]");

    private void BuildAllProperties(BuilderState state)
    {
        for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
        {
            BuildPropery(state, state.TypeInfo.Properties[i], i);
            state.AppendLine();
        }
    }

    private void BuildClassBody(BuilderState state)
    {
        BuildConstructor(state);
        state.AppendLine();
        BuildAllProperties(state);
        BuildApplyPath(state);
    }

    private void BuildApplyPath(BuilderState state)
    {
        state.AppendLine($"public override {state.TypeInfo.SourceTypeName} ApplyPatch([System.Diagnostics.CodeAnalysis.AllowNull] {state.TypeInfo.SourceTypeName} input)");
        state.AppendLine("{");
        var bodyState = state.IncrementIdentation();
        CallConstructIfEmpty(bodyState, "input ??=", leaveOpen: false);
        SetInitOnlyProperties(bodyState);
        SetReadWriteProperties(bodyState);
        BuildApplyPatchEnumerations(bodyState);
        bodyState.AppendLine("return input;");
        state.AppendLine("}");
    }

    private void SetInitOnlyProperties(BuilderState state)
    {
        if (!state.TypeInfo.Properties.Any(x => x.IsInitOnly))
            return;
        CallConstructIfEmpty(state, "var tmp =", leaveOpen: true);
        state.AppendLine("{");
        var initializerState = state.IncrementIdentation();
        for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
        {
            var currentProperty = state.TypeInfo.Properties[i];
            if (currentProperty.IsInitOnly)
            {
                currentProperty.Builder?.BuildInitOnly(state, i);
            }
            else
            {
                // Copy old property values onto the new object
                initializerState.AppendLine($"{currentProperty.Name} = input.{currentProperty.Name},");
            }
        }
        state.AppendLine("};");
        state.AppendLine("input = tmp;");
    }

    private void SetReadWriteProperties(BuilderState state)
    {
        for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
        {
            var currentProperty = state.TypeInfo.Properties[i];
            if (!currentProperty.IsInitOnly)
                currentProperty.Builder?.BuildInstantiation(state, i);
        }
    }

    private static void BuildApplyPatchEnumerations(BuilderState state)
    {
        for (int i = 0; i < state.TypeInfo.Properties.Count; i++)
        {
            var currentProperty = state.TypeInfo.Properties[i];
            currentProperty.Builder?.BuildPatch(state);
        }
    }

    private void CallConstructIfEmpty(BuilderState state, string toInitialize, bool leaveOpen)
    {
        IEnumerable<string> parameters = Enumerable.Empty<string>();
        if (!state.TypeInfo.HasDefaultCtor)
        {
            parameters = Enumerable.Repeat("default", state.TypeInfo.CtorParameterLength);
        }
        var ending = leaveOpen ? "" : ";";
        state.AppendLine($"{toInitialize} new {state.TypeInfo.SourceTypeName}({string.Join(", ", parameters)}){ending}");
        return;
    }
}
