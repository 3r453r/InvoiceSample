using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Helpers
{
    public class CollectionWrapper<T> : ICollection<IInitializeBase> where T : IInitializeBase
    {
        private readonly ICollection<T> _innerCollection;

        public CollectionWrapper(ICollection<T> innerCollection)
        {
            _innerCollection = innerCollection ?? throw new ArgumentNullException(nameof(innerCollection));
        }

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => _innerCollection.IsReadOnly;

        public void Add(IInitializeBase item)
        {
            if (item is T typedItem)
            {
                _innerCollection.Add(typedItem);
            }
            else
            {
                throw new InvalidOperationException($"Item must be of type {typeof(T)}.");
            }
        }

        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(IInitializeBase item)
        {
            return item is T typedItem && _innerCollection.Contains(typedItem);
        }

        public void CopyTo(IInitializeBase[] array, int arrayIndex)
        {
            foreach (var item in _innerCollection)
            {
                array[arrayIndex++] = item;
            }
        }

        public IEnumerator<IInitializeBase> GetEnumerator()
        {
            foreach (var item in _innerCollection)
            {
                yield return item;
            }
        }

        public bool Remove(IInitializeBase item)
        {
            return item is T typedItem && _innerCollection.Remove(typedItem);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
