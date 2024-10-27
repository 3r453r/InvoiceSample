using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Initializable
{
    public interface IInitializable : IInitializeBase
    {
        void Initialize(object entityData);
    }

    public interface IInitializableWithResult : IInitializable
    {
        new object Initialize(object entityData);
    }
}
