using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public class InvoiceLine : ExternalDataDrivenEntity<(string InvoiceNumber, int Ordinal), IInvoiceLine, InvoiceLineExternalData>
        , IInvoiceLine
    {
        private IMapper? _mapper;
        public InvoiceLine(Invoice invoice)
        {
            Invoice = invoice;

            RegisterExternalChild(
               SalesOrderLine
               , il => il.SalesOrderLine
               , (p) => { SalesOrderLine = null; }
               , (p) => { SalesOrderLine = (SalesOrderLine)p; }
               , (data) => new SalesOrderLine(Invoice.SalesOrders.First(so => so.Number == data.SalesOrderLine!.Document.Number))
               , (data) => new SalesOrderLineExternalData 
               {
                   Mapper = _mapper ?? throw new ArgumentNullException(nameof(Mapper)),
               });

            RegisterExternalChild(
               WarehouseReleaseLine
               , il => il.WarehouseReleaseLine
               , (p) => { WarehouseReleaseLine = null; }
               , (p) => { WarehouseReleaseLine = (WarehouseReleaseLine)p; }
               , (data) => new WarehouseReleaseLine(Invoice.SalesOrders.SelectMany(so => so.WarehouseReleases)
                    .First(wr => wr.Number == data.WarehouseReleaseLine!.WarehouseRelease.Number))
               , (data) => new WarehouseReleaseLineExternalData
               {
                   Mapper = _mapper ?? throw new ArgumentNullException(nameof(Mapper)),
               });
        }

        private bool _initialized;

        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public WarehouseReleaseLine? WarehouseReleaseLine { get; set; }
        IWarehouseReleaseLine? IInvoiceLine.WarehouseReleaseLine => WarehouseReleaseLine;

        public Invoice Invoice { get; set; }
        public IDocument Document => Invoice;
        IInvoiceData IInvoiceLine.Invoice => Invoice;

        public SalesOrderLine? SalesOrderLine { get; set; }
        ISalesOrderLine? IInvoiceLine.SalesOrderLine => SalesOrderLine;

        protected override bool SelfInitialzed => _initialized;

        public override IInvoiceLine GetEntityData() => this;

        public override (string InvoiceNumber, int Ordinal) GetKey() => (Invoice.Number, Ordinal);

        protected override void SelfInitialize(IInvoiceLine entityData, InvoiceLineExternalData externalData)
        {
            _mapper = externalData.Mapper;
            _mapper.Map(entityData, this);
        }

        object IEntityData.GetKey() => (Invoice.Number, Ordinal);
    }

    public record InvoiceLineExternalData
    {
        public required IMapper Mapper { get; set; }
    }
}
