using InvoiceSample.Domain.InvoiceAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain
{
    public interface IDocument
    {
        string Number { get; }
        IEnumerable<IDocumentLine> Lines { get; }
        Guid CustomerId { get; }
    }
}
