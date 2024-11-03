namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public record CollectionEntry
    {
        public required ICollection<IDataDrivenEntity> Collection { get; set; }
        public required Func<IEntityData, IEnumerable<IEntityData>> ChildCollectionDataSelector { get; set; }
        public required Func<IEntityData, IEntityData, IDataDrivenEntity> ChildCreator { get; set; }

    }

    public record ExternalCollectionEntry
    {
        public required ICollection<IExternalDataDrivenEntity> Collection { get; set; }
        public required Func<IEntityData, IEnumerable<IEntityData>> ChildCollectionDataSelector { get; set; }
        public required Func<object, object> ExternalDataProvider { get; set; }
        public required Func<IEntityData, IEntityData, IExternalDataDrivenEntity> ChildCreator { get; set; }

    }
}
