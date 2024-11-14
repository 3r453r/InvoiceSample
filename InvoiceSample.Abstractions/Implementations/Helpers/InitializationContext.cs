using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Helpers
{
    public class InitializationContext : IInitializationContext
    {
        private readonly Dictionary<(Type EntityType, object Key), IDataDrivenEntityBase> _initializedEntities = new();

        public bool IsInitialized(IDataDrivenEntityBase entity)
        {
            return _initializedEntities.ContainsKey((entity.GetType(), entity.GetKey()));
        }

        public void Add(IDataDrivenEntityBase entity)
        {
            _initializedEntities.Add((entity.GetType(), entity.GetKey()), entity);
        }

        public IDataDrivenEntityBase? GetInitialized((Type EntityType, object Key) key)
        {
            if (_initializedEntities.TryGetValue(key, out var entry))
            {
                return entry;
            }
            else 
            { 
                return null;
            }
             
        }
    }
}
