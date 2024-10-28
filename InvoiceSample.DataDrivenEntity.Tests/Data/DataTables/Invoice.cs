using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;
using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.DataTables
{
    public class Invoice : Entity, IInvoiceData
    {
        public string Number { get; set; } = "";

        public IInvoicePrintData? Print { get; set; }

        public List<InvoiceLine> Lines { get; set; } = [];
        IEnumerable<IInvoiceLineData> IInvoiceData.Lines => Lines;

        public IEnumerable<IWarehouseMovementData> WarehouseReleases { get; set; } = [];

        public IEnumerable<IWarehouseMovementData> WarehouseReturns { get; set; } = [];

        public DictionaryValue Status { get; set; } = new DictionaryValue();
        IDictionaryValueData? IInvoiceData.Status => Status;

        public DictionaryValue? InvoicingProcess { get; set; }
        IDictionaryValueData? IInvoiceData.InvoicingProcess => InvoicingProcess;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public Guid GetKey() => Id;
        object IEntityData.GetKey() => Id;
    }
}
