using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class ChildlessSelfDataDataDrivenEntity<TEntity, TKey, TEntityData> : SelfDataDataDrivenEntity<TEntity, TKey, TEntityData>
        where TEntity : TEntityData, new()
        where TKey : notnull
        where TEntityData : IEntityData<TKey>
    {
        public override IEnumerable<ChildEntry> ChildEntries => [];
        public override IEnumerable<CollectionEntry> CollectionEntries => [];
        protected override IInitializeBase CreateChild(string selector)
        {
            throw new InvalidOperationException();
        }

        public override void ClearChild(string selector)
        {
        }
    }
}
