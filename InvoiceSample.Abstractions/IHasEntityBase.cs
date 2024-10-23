using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasEntity
    {
        IInitializable ChildEntity { get; }
        object GetChildEntityData(object parentEntityData, object childKey);
        object Selector { get; }
    }

    public interface IHasEntityWithResult : IHasEntity
    {
        new IInitializableWithResult ChildEntity { get; }
    }

    public interface IHasEntities
    {
        ICollection<IInitializable> ChildEntities { get; }
        IEnumerable<object> GetChildrenEntityData(object parentEntityData);
        object Selector { get; }
    }

    public interface IHasEntitiesWithResult : IHasEntities
    {
        new ICollection<IInitializableWithResult> ChildEntities { get; }
    }

    public interface IHasExternalEntity
    {
        IExternallyInitializable ChildEntity { get; }
        object? GetExternalData(object self, object selfData);
        object GetChildEntityData(object parentEntityData, object childKey);
        object Selector { get; }
    }

    public interface IHasExternalEntityWithResult : IHasExternalEntity
    {
        new IExternallyInitializableWithResult ChildEntity { get; }
    }

    public interface IHasExternalEntities
    {
        ICollection<IExternallyInitializable> ChildEntities { get; }
        object? GetExternalData(object self, object selfData);
        object Selector { get; }
    }

    public interface IHasExternalEntitiesWithResult : IHasExternalEntities
    {
        new ICollection<IExternallyInitializableWithResult> ChildEntities { get; }
    }
}
