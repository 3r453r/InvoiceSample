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

        public bool Equals(IDictionaryDefinitionData? other) => other is not null && Id == other.Id ;

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
