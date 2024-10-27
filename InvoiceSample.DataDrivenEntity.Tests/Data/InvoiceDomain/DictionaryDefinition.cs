using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    public interface IDictionaryDefinitionData : IBaseEntityData
    {
        IEnumerable<IDictionaryValueData> Values { get; }
    }

    public class DictionaryDefinition : DataDrivenEntity<DictionaryDefinition, Guid, IDictionaryDefinitionData>
        , IDictionaryDefinitionData
    {
        private bool _initialized;

        public DictionaryDefinition()
        {
            RegisterChildCollection<DictionaryValue, Guid, IDictionaryValueData, IDictionaryDefinitionData, Guid>(Values, pd => pd.Values, () => new DictionaryValue());
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        protected override bool SelfInitialzed => _initialized;

        public List<DictionaryValue> Values { get; set; } = [];
        IEnumerable<IDictionaryValueData> IDictionaryDefinitionData.Values => Values;

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

        public override IDictionaryDefinitionData GetEntityData() => this;

        public override Guid GetKey() => Id;
    }
}
