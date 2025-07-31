using InvoiceSample.DataDrivenEntity.Implementations.Helpers;

namespace InvoiceSample.DataDrivenEntity
{
    /// <summary>
    /// Base interface for all data-driven entities
    /// </summary>
    public interface IDataDrivenEntityBase
    {
        /// <summary>
        /// Gets whether the entity is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets or sets whether the entity is new
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Gets the entity data
        /// </summary>
        object GetEntityData();

        /// <summary>
        /// Gets the entity key
        /// </summary>
        object GetKey();

        /// <summary>
        /// Gets all related entities
        /// </summary>
        IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false);
    }

    /// <summary>
    /// Interface for data-driven entities
    /// </summary>
    public interface IDataDrivenEntity : IDataDrivenEntityBase
    {
        /// <summary>
        /// Initializes the entity with the given data
        /// </summary>
        void Initialize(object entityData, IInitializationContext? context, bool isNew);
    }

    /// <summary>
    /// Generic interface for strongly-typed data-driven entities
    /// </summary>
    public interface IDataDrivenEntity<TKey, TEntityData> : IDataDrivenEntity
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        // Note: no redeclaration of GetEntityData or GetKey methods
        // This avoids ambiguity with the base interface

        /// <summary>
        /// Initializes the entity with the given data
        /// </summary>
        void Initialize(TEntityData entityData, IInitializationContext? context = null, bool isNew = false);
    }

    /// <summary>
    /// Interface for external data-driven entities
    /// </summary>
    public interface IExternalDataDrivenEntity : IDataDrivenEntityBase
    {
        /// <summary>
        /// Initializes the entity with the given data and external data
        /// </summary>
        void Initialize(object entityData, object externalData, IInitializationContext? context, bool isNew);
    }

    /// <summary>
    /// Generic interface for external data-driven entities
    /// </summary>
    public interface IDataDrivenEntity<TKey, TEntityData, TExternalData> : IExternalDataDrivenEntity
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        // Note: no redeclaration of GetEntityData or GetKey methods
        // This avoids ambiguity with the base interface

        /// <summary>
        /// Initializes the entity with the given data and external data
        /// </summary>
        void Initialize(TEntityData entityData, TExternalData externalData, IInitializationContext? context = null, bool isNew = false);
    }
}