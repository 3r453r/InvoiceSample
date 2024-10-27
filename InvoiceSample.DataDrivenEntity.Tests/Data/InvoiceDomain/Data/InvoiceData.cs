using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;

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

        public BaseInvoiceData Copy()
        {
            return new BaseInvoiceData
            {
                Id = this.Id,
                Name = this.Name,
                Created = this.Created,
                CreatedBy = this.CreatedBy,
                Number = this.Number
            };
        }
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
                other.WarehouseReleases.OrderBy(v => v.GetKey())
            )
            && WarehouseReturns.OrderBy(v => v.GetKey()).SequenceEqual(
                other.WarehouseReturns.OrderBy(v => v.GetKey())
            )
            && Lines.OrderBy(v => v.GetKey()).SequenceEqual(
                other.Lines.OrderBy(v => v.GetKey())
            )
            && ((Print is null && other.Print is null)
                || (Print is not null && Print.Equals(other.Print)))
            && ((Status is null && other.Status is null)
                || (Status is not null && Status.Equals(other.Status)))
            && ((InvoicingProcess is null && other.InvoicingProcess is null)
                || (InvoicingProcess is not null && InvoicingProcess.Equals(other.InvoicingProcess)))
            ;

        public InvoicePrintData? Print { get; set; }
        public List<InvoiceLineData> Lines { get; set; } = [];
        public List<WarehouseMovementData> WarehouseReleases { get; set; } = [];
        public List<WarehouseMovementData> WarehouseReturns { get; set; } = [];

        public DictionaryValueData? Status { get; set; }
        public DictionaryValueData? InvoicingProcess { get; set; }

        string IInvoiceData.Number => Number;

        public InvoiceData Copy()
        {
            // Create a copy of BaseInvoiceData
            var baseInvoiceDataCopy = new BaseInvoiceData
            {
                Id = this.Id,
                Name = this.Name,
                Created = this.Created,
                CreatedBy = this.CreatedBy,
                Number = this.Number
            };

            // Copy Print
            InvoicePrintData? printCopy = this.Print?.Copy();

            // Copy Lines
            var linesCopy = this.Lines.Select(line => line.Copy()).ToList();

            // Copy WarehouseReleases
            var releasesCopy = this.WarehouseReleases.Select(release => release.Copy()).ToList();

            // Copy WarehouseReturns
            var returnsCopy = this.WarehouseReturns.Select(ret => ret.Copy()).ToList();

            // Copy Status
            DictionaryValueData? statusCopy = this.Status?.Copy();

            // Copy InvoicingProcess
            DictionaryValueData? invoicingProcessCopy = this.InvoicingProcess?.Copy();

            // Create new InvoiceData instance
            var copy = new InvoiceData(baseInvoiceDataCopy, printCopy, linesCopy, releasesCopy, returnsCopy, statusCopy, invoicingProcessCopy);

            return copy;
        }

        IInvoicePrintData? IInvoiceData.Print => Print;

        IEnumerable<IInvoiceLineData> IInvoiceData.Lines => Lines;

        IEnumerable<IWarehouseMovementData> IInvoiceData.WarehouseReleases => WarehouseReleases;

        IEnumerable<IWarehouseMovementData> IInvoiceData.WarehouseReturns => WarehouseReturns;

        IDictionaryValueData? IInvoiceData.Status => Status;

        IDictionaryValueData? IInvoiceData.InvoicingProcess => InvoicingProcess;
    }
}
