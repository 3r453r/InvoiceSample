using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public abstract class Invoice : ExternalDataDrivenEntity<Guid, IInvoiceData, IMapper>
        , IInvoiceData
    {
        private bool _initialized;

        protected readonly List<InvoiceLine> _lines = [];
        protected readonly List<VatSum> _vatSums = [];
        protected readonly List<SalesOrder> _salesOrders = [];
        protected IMapper? _mapper;

        protected Invoice()
        {
            RegisterExternalChildCollection<InvoiceLine, int, IInvoiceLine, InvoiceLineExternalData>(
                    _lines
                    , d => d.Lines
                    , (_, _) => new InvoiceLine()
                    , (_) => new InvoiceLineExternalData { Invoice = this, Mapper = Mapper}
                );
            RegisterExternalChildCollection<VatSum, VatRate, IVatSum, IMapper>(
                    _vatSums
                    , d => d.VatSums
                    , (_, _) => new VatSum()
                    , (_) => Mapper
                );
            RegisterExternalChildCollection<SalesOrder, string, ISalesOrderData, IMapper>(
                    _salesOrders
                    , d => d.SalesOrders
                    , (_, _) => new SalesOrder()
                    , (_) => Mapper
                );
        }

        protected Invoice(IMapper mapper) : this()
        {
            _mapper = mapper;
        }

        public Invoice(ISalesOrderData salesOrderData, IMapper mapper) : this(mapper)
        {
            Number = $"I/{salesOrderData.Number}";
            CustomerId = salesOrderData.CustomerId;
            State = InvoiceState.Draft;
            Type = salesOrderData.AutoInvoice ? InvoiceType.Automatic : InvoiceType.Periodic;

            var salesOrder = new SalesOrder();
            salesOrder.Initialize(salesOrderData, Mapper);
            _salesOrders.Add(salesOrder);
        }

        protected override void SelfInitialize(IInvoiceData entityData, IMapper externalData)
        {
            _mapper = externalData;
            _mapper.Map(entityData, this);
            _initialized = true;
        }

        protected override bool SelfInitialzed => _initialized;

        public override Guid GetKey() => Id;

        protected IMapper Mapper => _mapper ?? throw new ArgumentNullException(nameof(Mapper));

        public Guid Id { get; protected set; }

        public string Number { get; protected set; } = "";

        public decimal NetValue { get; protected set; }

        public decimal VatValue { get; protected set; }

        public decimal GrossValue { get; protected set; }

        public Guid CustomerId { get; protected set; }

        public InvoiceState State { get; protected set; }

        public InvoiceType Type { get; protected set; }

        public IEnumerable<IInvoiceLine> Lines => _lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        public IEnumerable<IVatSum> VatSums => _vatSums;

        public IEnumerable<SalesOrder> SalesOrders => _salesOrders;
        IEnumerable<ISalesOrderData> IInvoiceData.SalesOrders => _salesOrders;


        public override IInvoiceData GetEntityData() => this;

        public abstract bool IsReadyToComplete();

        public abstract void Complete();

        public virtual void UpdateLines(IWarehouseReleaseData warehouseReleaseData)
        {
            if(State != InvoiceState.Draft)
            {
                throw new BusinessRuleException("Only drafts can be modified");
            }

            var salesOrder = _salesOrders.First(so => so.Number == warehouseReleaseData.SalesOrderNumber);
            var warehouseRelease = salesOrder.WarehouseReleases.FirstOrDefault(wr => wr.Number == warehouseReleaseData.Number);
            if(warehouseRelease is null)
            {
                warehouseRelease = salesOrder.WarehouseReleaseIssued(warehouseReleaseData);
            }
            else
            {
                warehouseRelease.Initialize(warehouseReleaseData, Mapper);
            }

            List<int> existingOrdinals = [];
            
            foreach(var line in _lines.Where(l => l.WarehouseReleaseLine?.WarehouseRelease.Number == warehouseRelease.Number)
                .ToArray())
            {
                var newLine = warehouseRelease.Lines.FirstOrDefault(l => l.Ordinal  == line.Ordinal);
                if (newLine is null)
                {
                    _lines.Remove(line);
                }
                else 
                { 
                    existingOrdinals.Add(line.Ordinal);
                }
            }

            var i = 1;
            var maxOrdinal = _lines.Any() ? _lines.Max(l => l.Ordinal) : 0;
            _lines.AddRange(warehouseRelease.Lines
                .Where(l => !existingOrdinals.Contains(l.Ordinal))
                .Select(l => new InvoiceLine
                {
                    Invoice = this,
                    Ordinal = maxOrdinal + i++,
                    GrossValue = l.GrossValue,
                    NetValue = l.NetValue,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    VatRate = l.VatRate,
                    VatValue = l.VatValue,
                    WarehouseReleaseLine = l,
                    SalesOrderLine = salesOrder.Lines.FirstOrDefault(sol => sol.Ordinal == l.SalesOrderLineOrdinal)
                }));

            UpdateTotals();
        }

        protected void UpdateTotals()
        {
            NetValue = _lines.Sum(l => l.NetValue);
            VatValue = _lines.Sum(l => l.VatValue);
            GrossValue = _lines.Sum(l => l.GrossValue);

            UpdateVatSums();
        }

        protected void UpdateVatSums()
        {
            List<VatRate> existingVatRates = [];
            foreach (var group in _lines.GroupBy(l => l.VatRate)) 
            {
                var vatSum = _vatSums.FirstOrDefault(vs => vs.VatRate == group.Key);
                if (vatSum == null) 
                {
                    vatSum = new VatSum
                    {
                        VatRate = group.Key,
                        GrossValue = group.Sum(l => l.GrossValue),
                        NetValue = group.Sum(l => l.NetValue),
                        VatValue = group.Sum(l => l.VatValue)
                    };
                    _vatSums.Add(vatSum);
                }
                else
                {
                    vatSum.NetValue = group.Sum(l => l.NetValue);
                    vatSum.GrossValue = group.Sum(l => l.GrossValue);
                    vatSum.VatValue = group.Sum(l => l.VatValue);
                }

                existingVatRates.Add(group.Key);
            }

            foreach(var vatSum in _vatSums.Where(vs => !existingVatRates.Contains(vs.VatRate)).ToArray())
            {
                _vatSums.Remove(vatSum);
            }
        }

        object IEntityData.GetKey() => GetKey();
    }
}
