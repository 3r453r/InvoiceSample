using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Persistence
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByNumber(string number);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
