using System;
using System.Collections.Generic;
using System.Linq;

namespace LaDeak.JsonMergePatch.Abstractions;

public static class ITypeRepositoryExtensions
{
    public static ITypeRepository Extend(this ITypeRepository target, ITypeRepository source)
    {
        _ = target ?? throw new ArgumentNullException(nameof(target));
        _ = source ?? throw new ArgumentNullException(nameof(source));

        foreach (var item in source.GetAll() ?? Enumerable.Empty<KeyValuePair<Type, Type>>())
            if (!target.TryGet(item.Key, out _))
                target.Add(item.Key, item.Value);
        return target;
    }
}
