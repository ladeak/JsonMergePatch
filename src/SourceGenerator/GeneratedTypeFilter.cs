using System.Linq;
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
            return typeInfo.SpecialType == SpecialType.None && !typeInfo.IsAnonymousType && !typeInfo.IsAbstract && !generic
            && typeInfo.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Constructor).All(x => x.Parameters.Count() == 0);
        }


        public static bool TryGetGeneratableType(ITypeSymbol typeInfo, out ITypeSymbol generatableType)
        {
            if (typeInfo is INamedTypeSymbol namedTypeInfo)
            {
                if (namedTypeInfo.IsGenericType && !namedTypeInfo.IsUnboundGenericType && namedTypeInfo.TypeArguments.Count() == 1 && namedTypeInfo.SpecialType != SpecialType.None)
                {
                    var genericTypeArgument = namedTypeInfo.TypeArguments.First();
                    if (IsGeneratableType(genericTypeArgument))
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
