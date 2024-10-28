using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SqlServerFixture : IDisposable
{
    private readonly string _connectionString;
    private readonly List<Type> _entityTypes = new();
    private readonly List<Action<ModelBuilder>> _entityConfigurations = new();
    private readonly List<object> _seedData = new();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerFixture"/> class with the specified connection string.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    public SqlServerFixture(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must be provided.", nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <summary>
    /// Registers an entity type to the DbContext.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to register.</typeparam>
    public void AddEntity<TEntity>() where TEntity : class
    {
        _entityTypes.Add(typeof(TEntity));
    }

    /// <summary>
    /// Registers an entity type with its configuration to the DbContext.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to register.</typeparam>
    /// <param name="configuration">The configuration action for the entity.</param>
    public void AddEntity<TEntity>(Action<EntityTypeBuilder<TEntity>> configuration) where TEntity : class
    {
        _entityTypes.Add(typeof(TEntity));
        _entityConfigurations.Add(modelBuilder =>
        {
            modelBuilder.Entity<TEntity>(configuration);
        });
    }

    /// <summary>
    /// Adds a general configuration action for the model.
    /// </summary>
    /// <param name="configuration">The configuration action.</param>
    public void Configure(Action<ModelBuilder> configuration)
    {
        _entityConfigurations.Add(configuration);
    }

    /// <summary>
    /// Adds seed data to be inserted into the database after creation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type of the data.</typeparam>
    /// <param name="entity">The entity instance to seed.</param>
    public void AddData<TEntity>(TEntity entity) where TEntity : class
    {
        if (!_entityTypes.Contains(typeof(TEntity)))
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} has not been registered. Use AddEntity<{typeof(TEntity).Name}>() to register it.");

        _seedData.Add(entity);
    }

    /// <summary>
    /// Creates and configures the DbContext.
    /// Ensures that the database is deleted and created.
    /// Inserts seed data if any.
    /// </summary>
    /// <returns>A configured <see cref="DbContext"/> instance.</returns>
    public DbContext CreateDbContext()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Connection string is not set.");

        if (_entityTypes.Count == 0)
            throw new InvalidOperationException("No entity types have been registered. Use AddEntity<TEntity>() to register entities.");

        var optionsBuilder = new DbContextOptionsBuilder<DynamicDbContext>();
        optionsBuilder.UseSqlServer(_connectionString);

        var context = new DynamicDbContext(optionsBuilder.Options, _entityTypes, _entityConfigurations);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (_seedData.Any())
        {
            foreach (var entity in _seedData)
            {
                context.Add(entity);
            }
            context.SaveChanges();
        }

        return context;
    }

    /// <summary>
    /// Disposes the fixture.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // No unmanaged resources to clean up in this fixture.
            _disposed = true;
        }
    }

    /// <summary>
    /// A dynamic DbContext that builds the model based on provided entity types and configurations.
    /// </summary>
    private class DynamicDbContext : DbContext
    {
        private readonly List<Type> _entityTypes;
        private readonly List<Action<ModelBuilder>> _entityConfigurations;

        public DynamicDbContext(DbContextOptions options, List<Type> entityTypes, List<Action<ModelBuilder>> entityConfigurations)
            : base(options)
        {
            _entityTypes = entityTypes;
            _entityConfigurations = entityConfigurations;
        }

        /// <summary>
        /// Configures the model by registering entity types and applying configurations.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in _entityTypes)
            {
                modelBuilder.Entity(entityType);
            }

            foreach (var config in _entityConfigurations)
            {
                config(modelBuilder);
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Dynamically creates a DbSet for a given entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>A <see cref="DbSet{TEntity}"/> for the entity.</returns>
        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }
    }
}