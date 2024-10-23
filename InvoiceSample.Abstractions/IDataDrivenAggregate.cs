using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenAggregate
    {
        IEnumerable<IHasEntity> Children { get; }
        IEnumerable<IHasExternalEntity> ExternalChildren { get; }
        IEnumerable<IHasEntities> ChildrenCollections { get; }
        IEnumerable<IHasExternalEntities> ExternalChildrenCollections { get; }

        void SetChild(object selector, IDataDrivenEntity? instance);
        IDataDrivenEntity GetChildInstance(object selector);
    }
}
