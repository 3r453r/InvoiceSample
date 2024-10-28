using AutoMapper;
using System.Reflection;

namespace InvoiceSample.DataDrivenEntity.Extensions
{
    public static class AutoMapperConfig
    {
        public static void GloballyIgnoreProperties(this IMapperConfigurationExpression mapperConfiguration, Action<IgnoreBuilder> config) 
        {
            var builder = new IgnoreBuilder();
            config(builder);

            mapperConfiguration.ShouldMapProperty = builder.Build();
        }
    }

    public class IgnoreBuilder
    {
        private List<Type> _ignoredTypes = [];

        public IgnoreBuilder Ignore<T>()
        {
            _ignoredTypes.Add(typeof(T));
            return this;
        }

        public IgnoreBuilder IgnoreCollections<T>()
        {
            _ignoredTypes.Add(typeof(IEnumerable<T>));
            return this;
        }

        public Func<PropertyInfo, bool> Build()
        {
            return (p) =>
            {
                var seed = false;
                foreach(var type in _ignoredTypes)
                {
                    seed = seed || p.PropertyType.IsAssignableTo(type);
                }
                return !seed;
            };
        }
    }
}
