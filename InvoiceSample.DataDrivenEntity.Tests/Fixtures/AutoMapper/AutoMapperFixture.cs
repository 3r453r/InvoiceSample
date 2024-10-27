using AutoMapper;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Fixtures.AutoMapper
{
    public class AutoMapperFixture
    {
        public IMapper Mapper { get; private set; }

        public AutoMapperFixture()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.ShouldMapProperty = p => !(
                    p.PropertyType.IsAssignableTo(typeof(IInitializeBase))
                    || p.PropertyType.IsAssignableTo(typeof(IEnumerable<IInitializeBase>))
                    );

                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            Mapper = config.CreateMapper();
        }
    }
}
