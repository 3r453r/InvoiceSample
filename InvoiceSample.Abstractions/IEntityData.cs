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

    public interface IChildEntityData : IEntityData
    {
        object Selector { get; }
    }

    public interface IEntityData<TKey>
        where TKey : notnull
    {
        TKey GetKey();
    }

    public interface IChildEntityData<TKey, TEntitySelector> : IEntityData<TKey>
        where TKey : notnull
        where TEntitySelector : notnull
    {
        TEntitySelector Selector { get; }
    }
}
