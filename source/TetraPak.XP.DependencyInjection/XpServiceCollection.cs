using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TetraPak.XP.DependencyInjection
{
    public sealed class XpServiceCollection : IServiceCollection
    {
        readonly IServiceCollection _serviceCollection;
        readonly List<XpServiceDelegate> _serviceDelegates;

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _serviceCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_serviceCollection).GetEnumerator();

        public void Add(ServiceDescriptor item) => _serviceCollection.Add(item);

        public void Clear() => _serviceCollection.Clear();

        public bool Contains(ServiceDescriptor item) => _serviceCollection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _serviceCollection.CopyTo(array, arrayIndex);

        public bool Remove(ServiceDescriptor item) => _serviceCollection.Remove(item);

        public int Count => _serviceCollection.Count;

        public bool IsReadOnly => _serviceCollection.IsReadOnly;

        public int IndexOf(ServiceDescriptor item) => _serviceCollection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => _serviceCollection.Insert(index, item);

        public void RemoveAt(int index) => _serviceCollection.RemoveAt(index);

        public ServiceDescriptor this[int index]
        {
            get => _serviceCollection[index];
            set => _serviceCollection[index] = value;
        }

        public IServiceProvider BuildServiceProvider(ServiceProviderOptions options)
            => new XpServiceProvider(this, options, _serviceDelegates);

        public void AddDelegate(XpServiceDelegate serviceDelegate) => _serviceDelegates.Add(serviceDelegate);

        internal IEnumerable<XpServiceDelegate> GetServiceDelegates() => _serviceDelegates;

        public XpServiceCollection(IServiceCollection? collection = null)
        {
            _serviceCollection = collection ?? new ServiceCollection();
            _serviceDelegates = new List<XpServiceDelegate>();
        }
    }
}