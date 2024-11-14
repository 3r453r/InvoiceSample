using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IInitializationContext
    {
        bool IsInitialized(IDataDrivenEntityBase entity);

        void Add(IDataDrivenEntityBase entity);

        IDataDrivenEntityBase? GetInitialized((Type EntityType, object Key) key);
    }
}
