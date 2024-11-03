using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public class SalesOrder : ExternalDataDrivenEntity<string, ISalesOrderData, IMapper>
        , ISalesOrderData
    {
        private readonly List<WarehouseRelease> _warehouseReleases = [];
        private IMapper? _mapper;
        private bool _initialized;

        public SalesOrder()
        {
            RegisterExternalChildCollection<SalesOrderLine, int, ISalesOrderLine, SalesOrderLineExternalData>(
                    Lines
                    , so => so.Lines
                    , (_, _) => new SalesOrderLine()
                    , (_) => new SalesOrderLineExternalData { 
                        Mapper = _mapper ?? throw new ArgumentNullException(nameof(_mapper)),
                        SalesOrder = this,
                    }
                );

            RegisterExternalChildCollection<WarehouseRelease, string, IWarehouseReleaseData, IMapper>(
                    _warehouseReleases
                    , so => so.WarehouseReleases
                    , (_, _) => new WarehouseRelease()
                    , (_) => _mapper ?? throw new ArgumentNullException(nameof(_mapper))
                );

            RegisterExternalChildCollection<Invoice, Guid, IInvoiceData, IMapper>(
                    Invoices
                    , so => so.Invoices
                    , (_, c) => c.Type == InvoiceType.Automatic ? new AutomaticInvoice() : new PeriodicInvoice()
                    , (_) => _mapper ?? throw new ArgumentNullException(nameof(_mapper))
                );
        }

        public SalesOrder(IMapper mapper) : this()
        {
            _mapper = mapper;
        }

        public WarehouseRelease WarehouseReleaseIssued(IWarehouseReleaseData warehouseReleaseData)
        {
            if (warehouseReleaseData.SalesOrderNumber != Number)
                throw new BusinessRuleException("invalid salesOrder for WR");

            if (_warehouseReleases.Any(wr => wr.Number == warehouseReleaseData.Number))
                throw new BusinessRuleException("WR already in salesOrder");

            var warehouseRelease = new WarehouseRelease(warehouseReleaseData, _mapper ?? throw new ArgumentNullException(nameof(_mapper)));
            _warehouseReleases.Add(warehouseRelease);

            return warehouseRelease;
        }

        public bool IsCompleted()
        {
            var productDeliveries = new Dictionary<Guid, decimal>();
            foreach (var wrLine in _warehouseReleases.SelectMany(wr => wr.Lines))
            {
                if (!productDeliveries.TryAdd(wrLine.ProductId, wrLine.Quantity))
                {
                    productDeliveries[wrLine.ProductId] += wrLine.Quantity;
                }
            }

            foreach (var line in Lines.Where(l => !l.IsService))
            {
                var deliveredQuantity = 0m;
                productDeliveries.TryGetValue(line.ProductId, out deliveredQuantity);
                if (deliveredQuantity < line.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public override ISalesOrderData GetEntityData() => this;

        public override string GetKey() => Number;

        protected override void SelfInitialize(ISalesOrderData entityData, IMapper externalData)
        {
            _mapper = externalData;
            _mapper.Map(entityData, this);
            _initialized = true;
        }

        object IEntityData.GetKey() => Number;

        public bool AutoInvoice { get; set; }

        public string Number { get; set; } = "";

        public Guid CustomerId { get; set; }

        public bool ServiceLinesInvoiced { get; set; }

        public List<SalesOrderLine> Lines { get; private set; } = [];
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;
        IEnumerable<ISalesOrderLine> ISalesOrderData.Lines => Lines;

        public IEnumerable<WarehouseRelease> WarehouseReleases => _warehouseReleases;
        IEnumerable<IWarehouseReleaseData> ISalesOrderData.WarehouseReleases => _warehouseReleases;

        public List<Invoice> Invoices { get; private set; } = [];
        IEnumerable<IInvoiceData> ISalesOrderData.Invoices => Invoices;

        protected override bool SelfInitialzed => _initialized;

    }
}
