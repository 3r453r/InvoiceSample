using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IEntityData
    {
        object GetKey();
    }

    public interface IEntityData<TKey> : IEntityData
        where TKey : notnull
    {
        new TKey GetKey();
    }
}
