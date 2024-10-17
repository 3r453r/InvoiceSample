using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public interface ISalesOrderData : IDocument
    {
        bool AutoInvoice { get; }
        bool ServiceLinesInvoiced { get; }

        new IEnumerable<ISalesOrderLine> Lines { get; }
        IEnumerable<IWarehouseReleaseData> WarehouseReleases { get; }
    }
}
