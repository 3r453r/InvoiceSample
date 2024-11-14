using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InvoiceSample.WebApi.Dtos
{
    public class SalesOrderRequested : ISalesOrderData
    {
        [Required]
        public bool AutoInvoice { get; set; }

        [JsonIgnore]
        public bool ServiceLinesInvoiced { get; set; }

        [Required]
        public List<SalesOrderLine> Lines { get; set; } = [];
        [JsonIgnore]
        IEnumerable<IDocumentLine> IDocument.Lines => Lines.Select(l => new SalesOrderLineData(l, this));
        [JsonIgnore]
        IEnumerable<ISalesOrderLine> ISalesOrderData.Lines => Lines.Select(l => new SalesOrderLineData(l, this));

        [JsonIgnore]
        public IEnumerable<IWarehouseReleaseData> WarehouseReleases => Enumerable.Empty<IWarehouseReleaseData>();

        [Required]
        public required string Number { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public IEnumerable<IInvoiceData> Invoices => [];

        public string GetKey() => Number;

        object IEntityData.GetKey() => Number;
    }

    public class SalesOrderLineData : ISalesOrderLine
    {
        public SalesOrderLineData(SalesOrderLine salesOrderLine, ISalesOrderData salesOrderData)
        {
            SalesOrder = salesOrderData;
            IsService = salesOrderLine.IsService;
            Ordinal = salesOrderLine.Ordinal;
            NetValue = salesOrderLine.NetValue;
            VatValue = salesOrderLine.VatValue;
            GrossValue = salesOrderLine.GrossValue;
            ProductId = salesOrderLine.ProductId;
            Quantity = salesOrderLine.Quantity;
            VatRate = salesOrderLine.VatRate;
        }

        public ISalesOrderData SalesOrder { get; set; }

        public bool IsService { get; set; }

        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public IDocument Document => SalesOrder;

        (string SalesOrderNumber, int Ordinal) IEntityData<(string SalesOrderNumber, int Ordinal)>.GetKey()
        {
            return (SalesOrder.Number, Ordinal);
        }

        object IEntityData.GetKey() => (SalesOrder.Number, Ordinal);
    }

    public class SalesOrderLine
    {
        [Required]

        public bool IsService { get; set; }

        [Required]
        [Range(1, 10000)]
        public int Ordinal { get; set; }

        [Required]

        public decimal NetValue { get; set; }

        [Required]

        public decimal VatValue { get; set; }

        [Required]

        public decimal GrossValue { get; set; }

        [Required]

        public Guid ProductId { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]

        public VatRate VatRate { get; set; }
    }
}
