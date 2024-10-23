using InvoiceSample.Persistence.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Extensions
{
    public static class CollectionExtensions
    {
        public static void UpdateCollectionTest<TEntity, TEntityData, TEntityKey>(
            this ICollection<TEntity> collection
            , IEnumerable<TEntityData> entityDatas
            , DbContext dbContext
            , Func<TEntityData, TEntity> entityGenerator
            , Action<TEntity, TEntityData> entityUpdater
            , Func<TEntity, TEntityKey> entityKeySelector
            , Func<TEntityData, TEntityKey> dataKeySelector) where TEntity : Entity
        {

        }

        public static void UpdateCollection<TEntity, TEntityData, TEntityKey>(
            this ICollection<TEntity> collection
            , IEnumerable<TEntityData> entityDatas
            , DbContext dbContext
            , Func<TEntityData, TEntity> entityGenerator
            , Action<TEntity, TEntityData> entityUpdater
            , Func<TEntity, TEntityKey> entityKeySelector
            , Func<TEntityData, TEntityKey> dataKeySelector) where TEntity : Entity
        {
            List<TEntityKey> existingIds = [];
            var dbSet = dbContext.Set<TEntity>();

            foreach(var entityData in entityDatas)
            {
                var dataKey = dataKeySelector(entityData);
                if(dataKey is null)
                {
                    throw new ArgumentNullException("data key is null");
                }

                existingIds.Add(dataKey);

                var entity = collection.FirstOrDefault(entity => dataKey.Equals(entityKeySelector(entity)));

                if (entity != null) 
                { 
                    entityUpdater(entity, entityData);
                }
                else
                {
                    entity = entityGenerator(entityData);
                    collection.Add(entity);
                }
            }

            foreach(var entityToRemove in collection
                .Where(e => !existingIds.Contains(entityKeySelector(e)))
                .ToArray())
            {
                dbSet.Remove(entityToRemove);
                collection.Remove(entityToRemove);
            }

            foreach(var entity in collection)
            {
                var entityKey = entityKeySelector(entity);
                if (entityKey is null)
                    throw new ArgumentNullException("entity key is null");

                var entityData = entityDatas.First(data => entityKey.Equals(dataKeySelector(data)));
                entity.UpdateCollections(entityData, dbContext);
            }
        }
    }
}
