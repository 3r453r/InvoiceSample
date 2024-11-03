namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public record ChildEntry
    {
        public IDataDrivenEntity? Entity { get; set; }

        public required Func<IEntityData, IEntityData?> ChildDataSelector { get; set; }

        public required Action<IDataDrivenEntity> RemoveChild {  get; set; }
        public required Action<IDataDrivenEntity> SetChild { get; set; }

        public required Func<IEntityData, IDataDrivenEntity> ChildCreator { get; set; }
    }

    public record ExternalChildEntry
    {
        public IExternalDataDrivenEntity? Entity { get; set; }

        public required Func<IEntityData, IEntityData?> ChildDataSelector { get; set; }

        public required Action<IExternalDataDrivenEntity> RemoveChild { get; set; }
        public required Action<IExternalDataDrivenEntity> SetChild { get; set; }

        public required Func<object, object> ExternalDataProvider { get; set; }

        public required Func<IEntityData, IExternalDataDrivenEntity> ChildCreator { get; set; }
    }
}
