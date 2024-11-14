using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public interface IInvoiceLine : IDocumentLine, IEntityData<(string InvoiceNumber, int Ordinal)>
    {
        IWarehouseReleaseLine? WarehouseReleaseLine { get; }
        ISalesOrderLine? SalesOrderLine { get; }
        IInvoiceData Invoice {  get; }
    }
}