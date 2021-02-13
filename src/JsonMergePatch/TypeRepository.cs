using System;
using System.Collections.Generic;

namespace LaDeak.JsonMergePatch
{
    public sealed class TypeRepository : ITypeRepository
    {
        private Dictionary<Type, Type> _repository = new Dictionary<Type, Type>();

        public void Add<TSource, TWrapper>() where TWrapper :  Patch<TSource>
        {
            _repository.Add(typeof(TSource), typeof(TWrapper));
        }

        public bool TryGet(Type source, out Type wrapper)
        {
            return _repository.TryGetValue(source, out wrapper);
        }
    }
}
