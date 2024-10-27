using AutoMapper;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Initializable;
using InvoiceSample.DataDrivenEntity.Tests.Data.AutoMapperData;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using InvoiceSample.DataDrivenEntity.Tests.Fixtures.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    [Collection("AutoMapper")]
    public class AutoMapperFixtureTests
    {
        private readonly IMapper _mapper;

        public AutoMapperFixtureTests(AutoMapperFixture autoMapper)
        {
            _mapper = autoMapper.Mapper;
        }
        [Fact]
        public void AutoMapper_Doesnt_Map_DataDrivenEntities() 
        { 
            var parent = new Parent(_mapper);
            var data = new ParentData();

            parent.Initialize(data);

            Assert.Null(parent.DataDrivenChild);
            Assert.Empty(parent.DataDrivenChildren);
            Assert.NotNull(parent.Child);
            Assert.NotEmpty(parent.Children);
        }
    }
}
