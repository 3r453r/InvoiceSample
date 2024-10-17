using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public interface IInvoiceLine : IDocumentLine
    {
        IWarehouseReleaseLine? WarehouseReleaseLine { get; }
        ISalesOrderLine? SalesOrderLine { get; }
        IInvoiceData Invoice {  get; }
    }
}