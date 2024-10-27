using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Initializable
{
    public interface IExternallyInitializable : IInitializeBase
    {
        void Initialize(object entityData, object externalData);
    }

    public interface IExternallyInitializableWithResult : IExternallyInitializable
    {
        new object Initialize(object entityData, object externalData);
    }
}
