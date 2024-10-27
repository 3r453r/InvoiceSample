using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntityData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data
{
    public class DictionaryDefinitionData : IDictionaryDefinitionData, IEquatable<IDictionaryDefinitionData>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public List<DictionaryValueData> Values { get; set; } = [];

        IEnumerable<IDictionaryValueData> IDictionaryDefinitionData.Values => Values;

        IEnumerable<IDictionaryValueData> IHasEntityDataCollection<IDictionaryValueData, Guid>.Children => Values;

        IEnumerable<(IEntityData? Entity, string Selector)> IAggregateEntityData.ChildrenData => [];

        IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> IAggregateEntityData.ChildrenCollectionsData => [(Values, nameof(Values))];

        public bool Equals(IDictionaryDefinitionData? other) =>
            other is not null && Id == other.Id && Name == other.Name
            && Created == other.Created && CreatedBy == other.CreatedBy 
            && Values.OrderBy(v => v.Id).SequenceEqual(other.Values.OrderBy(v => v.Id));

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
