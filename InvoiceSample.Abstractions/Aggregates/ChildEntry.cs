namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    internal record ChildEntry
    {
        public IDataDrivenEntity? Entity { get; set; }

        public required Func<IEntityData, IEntityData?> ChildDataSelector { get; set; }

        public required Action<IDataDrivenEntity> RemoveChild {  get; set; }
        public required Action<IDataDrivenEntity> SetChild { get; set; }

        public required Func<IDataDrivenEntity> ChildCreator { get; set; }
    }

    internal record ExternalChildEntry
    {
        public IExternalDataDrivenEntity? Entity { get; set; }

        public required Func<IEntityData, IEntityData?> ChildDataSelector { get; set; }

        public required Action<IExternalDataDrivenEntity> RemoveChild { get; set; }
        public required Action<IExternalDataDrivenEntity> SetChild { get; set; }

        public required Func<object> ExternalDataProvider { get; set; }

        public required Func<IExternalDataDrivenEntity> ChildCreator { get; set; }
    }
}
