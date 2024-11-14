using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    public interface IInvoiceData : IBaseEntityData
    { 
        string Number { get; }
        IInvoicePrintData? Print { get; }
        IEnumerable<IInvoiceLineData> Lines {  get; }
        IEnumerable<IWarehouseMovementData> WarehouseReleases { get; }
        IEnumerable<IWarehouseMovementData> WarehouseReturns { get; }
        IDictionaryValueData? Status { get; }
        IDictionaryValueData? InvoicingProcess { get; }
    }

    public class Invoice : DataDrivenEntity<Guid, IInvoiceData>
        , IInvoiceData
    {
        private bool _initialized;

        public Invoice()
        {
            RegisterChild(Print, pd => pd.Print, (p) => { Print = null; }, (p) => { Print = (InvoicePrint)p; }, (_) => new InvoicePrint());
            RegisterChildCollection<InvoiceLine, Guid, IInvoiceLineData>(Lines, pd => pd.Lines, (_, _) => new InvoiceLine());
            RegisterChildCollection<WarehouseMovement, Guid, IWarehouseMovementData>(WarehouseReleases, pd => pd.WarehouseReleases, (_, _) => new WarehouseMovement());
            RegisterChildCollection<WarehouseMovement, Guid, IWarehouseMovementData>(WarehouseReturns, pd => pd.WarehouseReturns, (_, _) => new WarehouseMovement());
            RegisterChild(Status, pd => pd.Status, (p) => { Status = null; }, (p) => { Status = (DictionaryValue)p; }, (_) => new DictionaryValue());
            RegisterChild(InvoicingProcess, pd => pd.InvoicingProcess, (p) => { InvoicingProcess = null; }, (p) => { InvoicingProcess = (DictionaryValue)p; }, (_) => new DictionaryValue());
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string Number { get; set; } = "";
        protected override bool SelfInitialzed => _initialized;
        object IEntityData.GetKey() => Id;

        protected override void SelfInitialize(IInvoiceData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            Number = entityData.Number;
            _initialized = true;
        }

        public override IInvoiceData GetEntityData() => this;

        public override Guid GetKey() => Id;

        public InvoicePrint? Print { get; set; }
        public List<InvoiceLine> Lines { get; set; } = [];
        public List<WarehouseMovement> WarehouseReleases { get; set; } = [];
        public List<WarehouseMovement> WarehouseReturns { get; set; } = [];

        public DictionaryValue? Status { get; set; }
        public DictionaryValue? InvoicingProcess { get; set; }

        IInvoicePrintData? IInvoiceData.Print => Print;

        IEnumerable<IInvoiceLineData> IInvoiceData.Lines => Lines;

        IEnumerable<IWarehouseMovementData> IInvoiceData.WarehouseReleases => WarehouseReleases;

        IEnumerable<IWarehouseMovementData> IInvoiceData.WarehouseReturns => WarehouseReturns;

        IDictionaryValueData? IInvoiceData.Status => Status;

        IDictionaryValueData? IInvoiceData.InvoicingProcess => InvoicingProcess;
    }
}
