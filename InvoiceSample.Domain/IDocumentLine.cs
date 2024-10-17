using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain
{
    public interface IDocumentLine
    {
        int Ordinal { get; }
        decimal NetValue { get; }
        decimal VatValue { get; }
        decimal GrossValue { get; }
        Guid ProductId { get; }
        decimal Quantity { get; }
        VatRate VatRate { get; }

        IDocument Document { get; }
    }

    public enum VatRate : byte
    {
        Zero,
        Seven,
        TwentyThree
    }
}
