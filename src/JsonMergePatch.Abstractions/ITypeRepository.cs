using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LaDeak.JsonMergePatch.Abstractions
{
    public interface ITypeRepository
    {
        /// <summary>
        /// Adds a type and corresponding wrapper to the type repository.
        /// </summary>
        /// <typeparam name="TSource">The user type.</typeparam>
        /// <typeparam name="TWrapper">Type that wraps the user type.</typeparam>
        void Add<TSource, TWrapper>() where TWrapper : Patch<TSource>;

        /// <summary>
        /// Returns the wrapper type for a registered user type.
        /// </summary>
        /// <param name="source">The user type.</param>
        /// <param name="wrapper">Type that wraps the user type.</param>
        /// <returns>True if the user type had a registration, otherwise false.</returns>
        bool TryGet(Type source, [NotNullWhen(true)] out Type wrapper);

        /// <summary>
        /// Adds a type and corresponding wrapper to the type repository.
        /// </summary>
        void Add(Type source, Type wrapper);

        /// <summary>
        /// Returns all registrations.
        /// </summary>
        /// <returns>Az enumeration of registered types and corresponding wrapper types.</returns>
        public IEnumerable<KeyValuePair<Type, Type>> GetAll();
    }
}