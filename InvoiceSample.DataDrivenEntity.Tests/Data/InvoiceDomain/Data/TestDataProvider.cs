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
            _data.Add(GetNewEntriesData());
            _data.Add(GeRemovedEntriesData());
            _data.Add(GetUpdateActionData());
        }

        private TestData GetNoActionData()
        {
            var originalData = new InvoiceData(new BaseInvoiceData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "noAction",
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

        private TestData GetNewEntriesData()
        {
            var dateTime = DateTime.Now;
            var originalData = new InvoiceData(new BaseInvoiceData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "NewEntries",
                Number = "b"
            }, null
            , new List<InvoiceLineData>
            {
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            }
            );

            var modifiedData = originalData.Copy();
            var ids = GetGuids(3);

            modifiedData.Print = new InvoicePrintData
            {
                Id = ids[0],
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            };

            modifiedData.Lines.Add(new InvoiceLineData
            {
                Id = ids[1],
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            });

            modifiedData.WarehouseReturns.Add(new WarehouseMovementData
            {
                Id = ids[2],
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            });

            Action<Invoice> modify = (invoice) => 
            {
                invoice.Print = new InvoicePrint
                {
                    Id = ids[0],
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                };

                invoice.Lines.Add(new InvoiceLine
                {
                    Id = ids[1],
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                });

                invoice.WarehouseReturns.Add(new WarehouseMovement
                {
                    Id = ids[2],
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                });
            };

            return new TestData
            {
                ModifiedData = modifiedData,
                OriginalData = originalData,
                Modify = modify,
            };
        }

        private TestData GeRemovedEntriesData()
        {
            var dateTime = DateTime.Now;
            var originalData = new InvoiceData(new BaseInvoiceData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "RemovedEntries",
                Number = "b"
            }, new InvoicePrintData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "a",
            }
            , new List<InvoiceLineData>
            {
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new InvoiceLineData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new List<WarehouseMovementData>
            {
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                },
                new WarehouseMovementData
                {
                    Id = Guid.NewGuid(),
                    Created = dateTime,
                    CreatedBy = 1,
                    Name = "a",
                }
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            }, new DictionaryValueData
            {
                Id = Guid.NewGuid(),
                Created = dateTime,
                CreatedBy = 1,
                Name = "a",
            }
            );

            var modifiedData = originalData.Copy();
            var ids = GetGuids(3);

            modifiedData.Print = null;

            modifiedData.Lines.RemoveAt(0);

            modifiedData.WarehouseReturns.RemoveAt(0);

            Action<Invoice> modify = (invoice) =>
            {
                invoice.Print = null;

                invoice.Lines.RemoveAt(0);

                invoice.WarehouseReturns.RemoveAt(0);
            };

            return new TestData
            {
                ModifiedData = modifiedData,
                OriginalData = originalData,
                Modify = modify,
            };
        }

        private TestData GetUpdateActionData()
        {
            var originalData = new InvoiceData(new BaseInvoiceData
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = 1,
                Name = "UpdateAction",
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

            var modifiedData = originalData.Copy();
            modifiedData.CreatedBy = 2;
            modifiedData.Name = "b";

            var secondLine = modifiedData.Lines.ElementAt(1);
            secondLine.Name = "c";
            secondLine.CreatedBy = 3;

            modifiedData.InvoicingProcess!.Name = "d";

            Action<Invoice> modify = (invoice) => 
            {
                invoice.CreatedBy = 2;
                invoice.Name = "b";

                var line = invoice.Lines.ElementAt(1);
                line.Name = "c";
                line.CreatedBy = 3;

                invoice.InvoicingProcess!.Name = "d";
            };

            return new TestData
            {
                ModifiedData = modifiedData,
                OriginalData = originalData,
                Modify = modify,
            };
        }

        private Guid[] GetGuids(int length)
        {
            Guid[] guidArray = Enumerable.Range(0, length)
                                         .Select(_ => Guid.NewGuid())
                                         .ToArray();

            return guidArray;
        }
    }
}
