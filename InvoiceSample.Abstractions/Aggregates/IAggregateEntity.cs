using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public interface IAggregateEntity
    {
        IEnumerable<ChildEntry> ChildEntries { get; }
        IEnumerable<CollectionEntry> CollectionEntries { get; }
        void ClearChild(string selector);
    }
}
