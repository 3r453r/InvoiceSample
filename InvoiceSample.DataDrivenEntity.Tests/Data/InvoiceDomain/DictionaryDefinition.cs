using InvoiceSample.DataDrivenEntity.HasEntity;
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
    using HasValues = IHasChildren<IDictionaryDefinitionData, Guid, DictionaryValue, Guid, IDictionaryValueData, DictionaryValue>;

    public interface IDictionaryDefinitionData : IBaseEntityData, IHasEntityDataCollection<IDictionaryValueData, Guid>
    {
        IEnumerable<IDictionaryValueData> Values { get; }
    }

    public class DictionaryDefinition : ReflectiveSelfDataDataDrivenEntity<DictionaryDefinition, Guid, IDictionaryDefinitionData>
        , IDictionaryDefinitionData
        , HasValues
    {
        private bool _initialized;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        protected override bool SelfInitialzed => _initialized;
        object IEntityData.GetKey() => Id;

        public IEnumerable<IDictionaryValueData> GetChildrenEntityData(IDictionaryDefinitionData entityData)
        {
            return entityData.Values;
        }

        protected override void SelfInitialize(IDictionaryDefinitionData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            _initialized = true;
        }

        public List<DictionaryValue> Values { get; set; } = [];
        IEnumerable<IDictionaryValueData> IDictionaryDefinitionData.Values => Values;
        IEnumerable<IDictionaryValueData> IHasEntityDataCollection<IDictionaryValueData, Guid>.Children => Values;
        ICollection<DictionaryValue> HasValues.ChildEntities => Values;

        public IEnumerable<(IEntityData? Entity, string Selector)> ChildrenData => [];

        public IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> ChildrenCollectionsData => [(Values, nameof(Values))];


    }
}
