using LaDeak.JsonMergePatch.Shared;
using System;

namespace JsonMergePatch.Shared
{
    public interface ITypeRepository
    {
        void TryAdd<TSource, TWrapper>() where TWrapper : Patch<TSource>;
        bool TryGet(Type source, out Type wrapper);
    }
}