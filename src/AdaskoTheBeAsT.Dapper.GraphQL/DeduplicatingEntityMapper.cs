using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.Dapper.GraphQL.Contexts;
using AdaskoTheBeAsT.Dapper.GraphQL.Interfaces;

namespace AdaskoTheBeAsT.Dapper.GraphQL
{
    /// <summary>
    /// An entity mapper that deduplicates as it maps.
    /// </summary>
    /// <remarks>
    /// The first entity found for each PrimaryKey is the object reference that will be returned.  All
    /// other entities that match the primary key will be ignored.
    /// </remarks>
    public abstract class DeduplicatingEntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        /// <summary>
        /// Sets a function that returns the primary key used to uniquely identify the entity.
        /// </summary>
        public Func<TEntityType, object>? PrimaryKey { get; set; }

        /// <summary>
        /// A cache used to hold previous entities that this mapper has seen.
        /// </summary>
        protected IDictionary<object, TEntityType> KeyCache { get; set; } = new Dictionary<object, TEntityType>();

        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="context">A context that contains information used to map Dapper objects.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public abstract TEntityType? Map(EntityMapContext context);

        /// <summary>
        /// Resolves the deduplicated entity.
        /// </summary>
        /// <param name="entity">The entity to deduplicate.</param>
        /// <returns>The deduplicated entity.</returns>
        protected virtual TEntityType? Deduplicate(TEntityType? entity)
        {
            if (entity == default(TEntityType))
            {
                return default;
            }

            if (PrimaryKey == null)
            {
                throw new InvalidOperationException("PrimaryKey selector is not defined, but is required to use DeduplicatingEntityMapper.");
            }

            // Get the primary key for this entity
            var primaryKey = PrimaryKey(entity) ??
                 throw new InvalidOperationException(
                     "A null primary key was provided, which results in an unpredictable state.");

            // Deduplicate the entity using available information
            if (KeyCache.TryGetValue(primaryKey, out var value))
            {
                // Get the duplicate entity
                entity = value;
            }
            else
            {
                // Cache a reference to the entity
                KeyCache[primaryKey] = entity;
            }

            return entity;
        }
    }
}
