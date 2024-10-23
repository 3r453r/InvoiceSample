using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IInitializable
    {
        void Initialize(object entityData);
        object GetKey();
    }

    public interface IInitializableWithResult : IInitializable
    {
        new object Initialize(object entityData);
    }

    public interface IExternallyInitializable
    {
        void Initialize(object entityData, object externalData);
        object GetKey();
    }

    public interface IExternallyInitializableWithResult : IExternallyInitializable
    {
        new object Initialize(object entityData, object externalData);
    }
}
