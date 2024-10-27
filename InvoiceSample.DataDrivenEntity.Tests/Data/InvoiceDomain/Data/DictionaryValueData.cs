using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data
{
    public class DictionaryValueData : IDictionaryValueData, IEquatable<IDictionaryValueData>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
        public bool Equals(IDictionaryValueData? other) => other is not null && Id == other.Id;

        public DictionaryValueData Copy()
        {
            return new DictionaryValueData
            {
                Id = this.Id,
                Name = this.Name,
                Created = this.Created,
                CreatedBy = this.CreatedBy
            };
        }
    }
}
