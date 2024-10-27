using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public interface IAggregateEntityData
    {
        IEnumerable<(IEntityData? Entity, string Selector)> ChildrenData { get; }
        IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> ChildrenCollectionsData { get; }
    }
}
