using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public record ChildEntry
    {
        public IInitializeBase? Entity { get; set; }
        public string Selector { get; init; }

        public ChildEntry(IInitializeBase? entity, string selector)
        {
            Entity = entity;
            Selector = selector;
        }
    }
}
