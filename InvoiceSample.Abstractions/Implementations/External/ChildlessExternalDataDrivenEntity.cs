using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.External
{
    public abstract class ChildlessExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData> : ExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
        where TExternalData : class
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
