using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public record CollectionEntry(ICollection<IInitializeBase> Collection, string CollectionSelector, string ChildSelector)
    {
    }
}
