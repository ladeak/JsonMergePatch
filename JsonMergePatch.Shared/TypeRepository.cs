using LaDeak.JsonMergePatch.Shared;
using System;
using System.Collections.Generic;

namespace LaDeak.JsonMergePatch.Shared
{
    public class TypeRepository : ITypeRepository
    {
        private Dictionary<Type, Type> _repository = new Dictionary<Type, Type>();

        public void TryAdd<TSource, TWrapper>() where TWrapper : Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public bool TryGet(Type source, out Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }
    }
}
