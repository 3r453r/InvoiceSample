using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data;
using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.DataTables
{
    public class DictionaryDefinitionData : Entity, IDictionaryDefinitionData
    {
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public List<DictionaryValueData> Values { get; set; } = [];
        IEnumerable<IDictionaryValueData> IDictionaryDefinitionData.Values => Values;

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
