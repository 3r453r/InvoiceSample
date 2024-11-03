using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public interface ISalesOrderData : IDocument, IEntityData<string>
    {
        bool AutoInvoice { get; }
        bool ServiceLinesInvoiced { get; }

        new IEnumerable<ISalesOrderLine> Lines { get; }
        IEnumerable<IWarehouseReleaseData> WarehouseReleases { get; }
        IEnumerable<IInvoiceData> Invoices { get; }
    }
}
