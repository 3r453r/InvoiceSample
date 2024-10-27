using System.Collections;

namespace InvoiceSample.DataDrivenEntity.Implementations.Helpers
{
    public class CollectionWrapper<T> : ICollection<IDataDrivenEntity> where T : IDataDrivenEntity
    {
        private readonly ICollection<T> _innerCollection;

        public CollectionWrapper(ICollection<T> innerCollection)
        {
            _innerCollection = innerCollection;
        }

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => _innerCollection.IsReadOnly;

        public void Add(IDataDrivenEntity item)
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

        public bool Contains(IDataDrivenEntity item)
        {
            return item is T typedItem && _innerCollection.Contains(typedItem);
        }

        public void CopyTo(IDataDrivenEntity[] array, int arrayIndex)
        {
            foreach (var item in _innerCollection)
            {
                array[arrayIndex++] = item;
            }
        }

        public IEnumerator<IDataDrivenEntity> GetEnumerator()
        {
            foreach (var item in _innerCollection)
            {
                yield return item;
            }
        }

        public bool Remove(IDataDrivenEntity item)
        {
            return item is T typedItem && _innerCollection.Remove(typedItem);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ExternalCollectionWrapper<T> : ICollection<IExternalDataDrivenEntity> where T : IExternalDataDrivenEntity
    {
        private readonly ICollection<T> _innerCollection;

        public ExternalCollectionWrapper(ICollection<T> innerCollection)
        {
            _innerCollection = innerCollection;
        }

        public int Count => _innerCollection.Count;

        public bool IsReadOnly => _innerCollection.IsReadOnly;

        public void Add(IExternalDataDrivenEntity item)
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

        public bool Contains(IExternalDataDrivenEntity item)
        {
            return item is T typedItem && _innerCollection.Contains(typedItem);
        }

        public void CopyTo(IExternalDataDrivenEntity[] array, int arrayIndex)
        {
            foreach (var item in _innerCollection)
            {
                array[arrayIndex++] = item;
            }
        }

        public IEnumerator<IExternalDataDrivenEntity> GetEnumerator()
        {
            foreach (var item in _innerCollection)
            {
                yield return item;
            }
        }

        public bool Remove(IExternalDataDrivenEntity item)
        {
            return item is T typedItem && _innerCollection.Remove(typedItem);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
