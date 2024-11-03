using InvoiceSample.DataDrivenEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public interface ISalesOrderLine : IDocumentLine, IEntityData<int>
    {
        ISalesOrderData SalesOrder { get; }
        bool IsService { get; }
    }
}
