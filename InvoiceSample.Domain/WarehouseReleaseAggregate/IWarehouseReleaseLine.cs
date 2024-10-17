using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public interface IWarehouseReleaseLine : IDocumentLine
    {
        IWarehouseReleaseData WarehouseRelease { get; }
        int? SalesOrderLineOrdinal { get; }
    }
}
