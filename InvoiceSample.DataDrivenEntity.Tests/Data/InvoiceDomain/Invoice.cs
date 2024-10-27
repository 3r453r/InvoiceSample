using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntityData;
using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    using HasPrint = HasEntity.IHasChild<IInvoiceData, Guid, InvoicePrint, Guid, IInvoicePrintData, InvoicePrint>;
    using HasLines = HasEntity.IHasChildren<IInvoiceData, Guid, InvoiceLine, Guid, IInvoiceLineData, InvoiceLine>;
    using HasWarehouseMovements = HasEntity.IHasMultipleEntityCollections<IInvoiceData, Guid, WarehouseMovement, Guid, IWarehouseMovementData, WarehouseMovement, string>;
    using HasDictionaryValues = HasEntity.IHasMultipleEntityInstances<IInvoiceData, Guid, DictionaryValue, Guid, IDictionaryValueData, DictionaryValue, string>;

    using HasPrintData = IHasEntityData<IInvoicePrintData, Guid>;
    using HasLinesData = IHasEntityDataCollection<IInvoiceLineData, Guid>;
    using HasWarehouseMovementsData = IHasMultipleEntityDataCollections<IWarehouseMovementData, Guid, string>;
    using HasDictionaryValuesData = IHasMultipleEntityDataInstances<IDictionaryValueData, Guid, string>;

    public interface IInvoiceData : IBaseEntityData
        , HasPrintData
        , HasLinesData
        , HasWarehouseMovementsData
        , HasDictionaryValuesData
    { 
        string Number { get; }
    }

    public class Invoice : ReflectiveSelfDataDataDrivenEntity<Invoice, Guid, IInvoiceData>
        , IInvoiceData
        , HasPrint
        , HasLines
        , HasWarehouseMovements
        , HasDictionaryValues
    {
        private bool _initialized;

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

        IEnumerable<IInvoiceLineData> HasLines.GetChildrenEntityData(IInvoiceData entityData) => Lines;

        IInvoicePrintData? HasPrint.GetChildEntityData(IInvoiceData entityData, Guid childKey) => Print;

        public InvoicePrint? Print { get; set; }
        public List<InvoiceLine> Lines { get; set; } = [];
        public List<WarehouseMovement> WarehouseReleases { get; set; } = [];
        public List<WarehouseMovement> WarehouseReturns { get; set; } = [];

        public DictionaryValue? Status { get; set; }
        public DictionaryValue? InvoicingProcess { get; set; }

        IEnumerable<(DictionaryValue? Entity, string Selector)> HasDictionaryValues.ChildInstances => [(Status, nameof(Status)),(InvoicingProcess, nameof(InvoicingProcess))];

        IEnumerable<(ICollection<WarehouseMovement> Collection, string Selector)> HasWarehouseMovements.ChildEntityCollections => [(WarehouseReleases, nameof(WarehouseReleases)), (WarehouseReturns, nameof(WarehouseReturns))];

        ICollection<InvoiceLine> HasLines.ChildEntities => Lines;

        InvoicePrint? HasPrint.Child { get { return Print; } set { Print = value; } }

        IInvoicePrintData? HasPrintData.Child => Print;

        IEnumerable<IInvoiceLineData> HasLinesData.Children => Lines;

        IEnumerable<(IEnumerable<IWarehouseMovementData> Collection, string Selector)> HasWarehouseMovementsData.ChildrenCollections => [(WarehouseReleases, nameof(WarehouseReleases)), (WarehouseReturns, nameof(WarehouseReturns))];

        IEnumerable<(IDictionaryValueData? ChildData, string Selector)> HasDictionaryValuesData.ChildInstances => [(Status, nameof(Status)), (InvoicingProcess, nameof(InvoicingProcess))];

        IEnumerable<(IEntityData? Entity, string Selector)> IAggregateEntityData.ChildrenData => [(Print, nameof(Print)), (Status, nameof(Status)), (InvoicingProcess, nameof(InvoicingProcess))];

        IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> IAggregateEntityData.ChildrenCollectionsData => [(WarehouseReleases, nameof(WarehouseReleases)), (WarehouseReturns, nameof(WarehouseReturns)), (Lines, nameof(Lines))];
    }
}
