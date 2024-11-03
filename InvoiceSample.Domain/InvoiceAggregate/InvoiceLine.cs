using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public class InvoiceLine : ExternalDataDrivenEntity<int, IInvoiceLine, InvoiceLineExternalData>
        , IInvoiceLine
    {
        private IMapper? _mapper;
        public InvoiceLine()
        {
            RegisterExternalChild<SalesOrderLine, int, ISalesOrderLine, SalesOrderLineExternalData>(
               SalesOrderLine
               , il => il.SalesOrderLine
               , (p) => { SalesOrderLine = null; }
               , (p) => { SalesOrderLine = (SalesOrderLine)p; }
               , (_) => new SalesOrderLine(), (data) => new SalesOrderLineExternalData 
               {
                   SalesOrder = Invoice.SalesOrders.First(so => so.Number == data.Invoice.Number),
                   Mapper = _mapper ?? throw new ArgumentNullException(nameof(Mapper)),
               });

            RegisterExternalChild<WarehouseReleaseLine, int, IWarehouseReleaseLine, WarehouseReleaseLineExternalData>(
               WarehouseReleaseLine
               , il => il.WarehouseReleaseLine
               , (p) => { WarehouseReleaseLine = null; }
               , (p) => { WarehouseReleaseLine = (WarehouseReleaseLine)p; }
               , (_) => new WarehouseReleaseLine()
               , (data) => new WarehouseReleaseLineExternalData
               {
                   WarehouseRelease = Invoice.SalesOrders.SelectMany(so => so.WarehouseReleases)
                    .First(wr => wr.Number == data.WarehouseReleaseLine!.WarehouseRelease.Number),
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

        public Invoice Invoice { get; set; } = new AutomaticInvoice();
        public IDocument Document => Invoice;
        IInvoiceData IInvoiceLine.Invoice => Invoice;

        public SalesOrderLine? SalesOrderLine { get; set; }
        ISalesOrderLine? IInvoiceLine.SalesOrderLine => SalesOrderLine;

        protected override bool SelfInitialzed => _initialized;

        public override IInvoiceLine GetEntityData() => this;

        public override int GetKey() => Ordinal;

        protected override void SelfInitialize(IInvoiceLine entityData, InvoiceLineExternalData externalData)
        {
            Invoice = externalData.Invoice;
            externalData.Mapper.Map(entityData, this);
        }

        object IEntityData.GetKey() => Ordinal;
    }

    public record InvoiceLineExternalData
    {
        public required Invoice Invoice { get; set; }
        public required IMapper Mapper { get; set; }
    }
}
