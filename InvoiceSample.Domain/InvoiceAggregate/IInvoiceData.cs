using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public interface IInvoiceData : IDocument
    {
        decimal NetValue { get; }
        decimal VatValue { get; }
        decimal GrossValue { get; }
        InvoiceState State { get; }
        InvoiceType Type { get; }
        new IEnumerable<IInvoiceLine> Lines { get; }
        IEnumerable<IVatSum> VatSums { get; } 
        IEnumerable<ISalesOrderData> SalesOrders { get; }
    }

    public enum InvoiceState : byte
    {
        Draft,
        ReadyToInvoice,
        Invoiced,
        Error
    }

    public enum InvoiceType : byte
    {
        Automatic,
        Periodic
    }
}
