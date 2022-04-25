﻿using System.Linq;
using Microsoft.CodeAnalysis;

namespace LaDeak.JsonMergePatch.SourceGenerator
{
    public static class GeneratedTypeFilter
    {
        public static SymbolDisplayFormat SymbolFormat = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeType, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.None);

        public static bool IsGeneratableType(ITypeSymbol typeInfo)
        {
            bool generic = false;
            if (typeInfo is INamedTypeSymbol namedTypeInfo)
                generic = namedTypeInfo.IsGenericType;

            // Check for System types that have SpecialType.None.
            string typeName = typeInfo.ToDisplayString();
            bool isNonSpecialSystemType = typeName == "System.Guid" || typeName == "System.DateOnly" || typeName == "System.TimeOnly";

            return typeInfo.SpecialType == SpecialType.None
                && !isNonSpecialSystemType
                && !typeInfo.IsAnonymousType
                && !typeInfo.IsAbstract
                && !generic && !typeInfo.IsStatic
                && typeInfo.TypeKind != TypeKind.Enum;
        }


        public static bool TryGetGeneratableType(ITypeSymbol typeInfo, out ITypeSymbol generatableType)
        {
            if (typeInfo is INamedTypeSymbol namedTypeInfo)
            {
                if (namedTypeInfo.IsGenericType && !namedTypeInfo.IsUnboundGenericType)
                {
                    ITypeSymbol? genericTypeArgument = null;
                    if (namedTypeInfo.TypeArguments.Length == 1 && namedTypeInfo.SpecialType != SpecialType.None)
                        genericTypeArgument = namedTypeInfo.TypeArguments.First();
                    else if (namedTypeInfo.TypeArguments.Length == 2 && namedTypeInfo.Name == "Dictionary" && namedTypeInfo.ContainingNamespace.ToDisplayString() == "System.Collections.Generic")
                    {
                        genericTypeArgument = namedTypeInfo.TypeArguments.Last();
                        if (genericTypeArgument is INamedTypeSymbol dictionaryValueType && dictionaryValueType.IsGenericType && dictionaryValueType.SpecialType == SpecialType.System_Nullable_T)
                            genericTypeArgument = dictionaryValueType.TypeArguments.First();
                    }
                    if (genericTypeArgument != null && IsGeneratableType(genericTypeArgument))
                    {
                        generatableType = genericTypeArgument;
                        return true;
                    }
                }
            }

            generatableType = typeInfo;
            return IsGeneratableType(typeInfo);
        }

        public static string SourceTypeName(ITypeSymbol typeInfo)
        {
            return typeInfo.ToDisplayString(SymbolFormat);
        }
    }
}
