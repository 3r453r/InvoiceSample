using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.DataTables
{
    public class InvoiceLineData : Entity, IInvoiceLineData
    {
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public DictionaryValue Status { get; set; } = new DictionaryValue();

        IDictionaryValueData IInvoiceLineData.Status => Status;

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
