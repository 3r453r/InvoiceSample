using AutoMapper;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSample.DataDrivenEntity.Extensions
{
    public abstract class EFRepository<TEntity, TKey, TEntityData>
    where TEntity : class, IDataDrivenEntity<TKey, TEntityData, IMapper>, IAggregateEntity<TKey, TEntityData>
    where TKey : notnull
    where TEntityData : IEntityData<TKey>
    {
        private readonly DbContext _dbContext;

        protected EFRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public abstract Task<TEntity?> FindByDataKey(TKey key);
        protected abstract TEntity CreateTableEntity();
        protected abstract Task SetCustomNavigations(TEntity entity, TEntityData entityData);

        protected async Task<TEntity> AddOrUpdateEntity(TEntityData entityData, IMapper mapper)
        {
            var tableEntity = await FindByDataKey(entityData.GetKey());
            if (tableEntity == null)
            {
                tableEntity = CreateTableEntity();
                await _dbContext.Set<TEntity>().AddAsync(tableEntity);
            }

            tableEntity.Initialize(entityData, mapper);

            var allEntties = tableEntity.GetAllEntities().ToList();
            foreach (var childEntity in tableEntity.GetAllEntities())
            {
                if (childEntity.IsNew)
                {
                    var entry = _dbContext.Entry(childEntity);
                    entry.State = EntityState.Added;
                }
            }

            await SetCustomNavigations(tableEntity, entityData);

            return tableEntity;
        }
    }
}
