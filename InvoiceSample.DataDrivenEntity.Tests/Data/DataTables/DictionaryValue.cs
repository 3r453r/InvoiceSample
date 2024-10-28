using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.DataTables
{
    public class DictionaryValue : Entity, IDictionaryValueData
    {
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
