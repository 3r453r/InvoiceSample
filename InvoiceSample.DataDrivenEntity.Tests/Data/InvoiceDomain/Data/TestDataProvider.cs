using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data
{
    public class TestData
    {
        public required InvoiceData OriginalData { get; set; }
        public required InvoiceData ModifiedData { get; set; }

        public required Action<Invoice> Modify { get; set; }
    }

    public class TestDataProvider : IEnumerable<object[]>
    {
        private List<TestData> _data = [];
        public IEnumerator<object[]> GetEnumerator() => _data.Select<TestData, object[]>(d => [d]).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        public TestDataProvider()
        {
            _data.Add(GetNoActionData());   
        }

        private TestData GetNoActionData()
        {
            var originalData = new InvoiceData(new BaseInvoiceData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "a",
                Number = "b"
            }, new InvoicePrintData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "a",
            }, new List<InvoiceLineData>
            {
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                },
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                },
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData 
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = DateTime.Now,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "a",
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "a",
            }
            );

            Action<Invoice> modify = (invoice) => { };

            return new TestData
            {
                ModifiedData = originalData,
                OriginalData = originalData,
                Modify = modify,
            };
        }
    }
}
