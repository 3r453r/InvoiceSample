using InvoiceSample.DataDrivenEntity.Tests.Fixtures.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Fixtures.SqlServer
{
    [CollectionDefinition("SqlServer")]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
    }
}
