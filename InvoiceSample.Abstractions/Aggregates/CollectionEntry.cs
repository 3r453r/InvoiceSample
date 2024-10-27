namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    internal record CollectionEntry
    {
        public required ICollection<IDataDrivenEntity> Collection { get; set; }
        public required Func<IEntityData, IEnumerable<IEntityData>> ChildCollectionDataSelector { get; set; }
        public required Func<IDataDrivenEntity> ChildCreator { get; set; }

    }

    internal record ExternalCollectionEntry
    {
        public required ICollection<IExternalDataDrivenEntity> Collection { get; set; }
        public required Func<IEntityData, IEnumerable<IEntityData>> ChildCollectionDataSelector { get; set; }
        public required Func<object> ExternalDataProvider { get; set; }
        public required Func<IExternalDataDrivenEntity> ChildCreator { get; set; }

    }
}
