using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntityData;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data
{
    public class BaseInvoiceData : IBaseEntityData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string Number { get; set; } = "";

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }

    public class InvoiceData : IInvoiceData, IEquatable<IInvoiceData>
    {
        public InvoiceData(BaseInvoiceData baseInvoiceData, InvoicePrintData? printData
            , List<InvoiceLineData> lineData
            , List<WarehouseMovementData> releasesData
            , List<WarehouseMovementData> returnsData
            , DictionaryValueData statusData
            , DictionaryValueData invoicingData)
        {
            Id = baseInvoiceData.Id;
            Name = baseInvoiceData.Name;
            Created = baseInvoiceData.Created;
            CreatedBy = baseInvoiceData.CreatedBy;
            Number = baseInvoiceData.Number;
            Print = printData;
            Lines = lineData;
            WarehouseReleases = releasesData;
            WarehouseReturns = returnsData;
            Status = statusData;
            InvoicingProcess = invoicingData;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public string Number { get; set; }

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;

        public bool Equals(IInvoiceData? other) =>
            other is not null && Id == other.Id && Name == other.Name
            && Created == other.Created && CreatedBy == other.CreatedBy
            && WarehouseReleases.OrderBy(v => v.GetKey()).SequenceEqual(
                other.ChildrenCollectionsData
                .Single(cc => cc.Selector == "WarehouseReleases").Collection.OrderBy(v => v.GetKey())
            )
            && WarehouseReturns.OrderBy(v => v.GetKey()).SequenceEqual(
                other.ChildrenCollectionsData
                .Single(cc => cc.Selector == "WarehouseReturns").Collection.OrderBy(v => v.GetKey())
            )
            && Lines.OrderBy(v => v.GetKey()).SequenceEqual(
                other.ChildrenCollectionsData
                .Single(cc => cc.Selector == "Lines").Collection.OrderBy(v => v.GetKey())
            )
            && ((Print is null && other.ChildrenData.Single(cd => cd.Selector == "Print").Entity is null) 
                || (Print is not null && Print.Equals(other.ChildrenData.Single(cd => cd.Selector == "Print").Entity as IInvoicePrintData)))
            && ((Status is null && other.ChildrenData.Single(cd => cd.Selector == "Status").Entity is null)
                || (Status is not null && Status.Equals(other.ChildrenData.Single(cd => cd.Selector == "Status").Entity as IDictionaryValueData)))
            && ((InvoicingProcess is null && other.ChildrenData.Single(cd => cd.Selector == "InvoicingProcess").Entity is null)
                || (InvoicingProcess is not null && InvoicingProcess.Equals(other.ChildrenData.Single(cd => cd.Selector == "InvoicingProcess").Entity as IDictionaryValueData)))
            ;

        public InvoicePrintData? Print { get; set; }
        public List<InvoiceLineData> Lines { get; set; } = [];
        public List<WarehouseMovementData> WarehouseReleases { get; set; } = [];
        public List<WarehouseMovementData> WarehouseReturns { get; set; } = [];

        public DictionaryValueData? Status { get; set; }
        public DictionaryValueData? InvoicingProcess { get; set; }

        string IInvoiceData.Number => Number;

        IInvoicePrintData? IHasEntityData<IInvoicePrintData, Guid>.Child => Print;

        IEnumerable<IInvoiceLineData> IHasEntityDataCollection<IInvoiceLineData, Guid>.Children => Lines;

        IEnumerable<(IEnumerable<IWarehouseMovementData> Collection, string Selector)> IHasMultipleEntityDataCollections<IWarehouseMovementData, Guid, string>.ChildrenCollections => [(WarehouseReleases, nameof(WarehouseReleases)), (WarehouseReturns, nameof(WarehouseReturns))];

        IEnumerable<(IDictionaryValueData? ChildData, string Selector)> IHasMultipleEntityDataInstances<IDictionaryValueData, Guid, string>.ChildInstances => [(Status, nameof(Status)), (InvoicingProcess, nameof(InvoicingProcess))];

        IEnumerable<(IEntityData? Entity, string Selector)> IAggregateEntityData.ChildrenData => [(Print, nameof(Print)), (Status, nameof(Status)), (InvoicingProcess, nameof(InvoicingProcess))];

        IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> IAggregateEntityData.ChildrenCollectionsData => [(WarehouseReleases, nameof(WarehouseReleases)), (WarehouseReturns, nameof(WarehouseReturns)), (Lines, nameof(Lines))];
    }
}
