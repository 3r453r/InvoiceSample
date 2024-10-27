using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Initializable
{
    public interface IInitializeBase
    {
        bool IsInitialized { get; }
        object GetKey();
    }
}
