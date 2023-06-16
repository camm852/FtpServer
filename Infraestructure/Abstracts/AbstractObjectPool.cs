using Domain.Interfaces;
using Infraestructure.Interfaces;
using System.Collections.Concurrent;

namespace Infraestructure.Abstracts
{
    public abstract  class AbstractObjectPool<T> : IObjectPool<T> where T : IPoolableObject
    {
        private ConcurrentBag<T> pool;
        private IObjectFactory<T> factory;
        private int maxConnections;
        private int currentConnections;

        public AbstractObjectPool(IObjectFactory<T> factory, int maxConnections)
        {
            this.pool = new ConcurrentBag<T>();
            this.factory = factory;
            this.maxConnections = maxConnections;
            this.currentConnections = 0;
            for (int i = 1; i <= maxConnections; i++)
            {
                T item = factory.CreateObject();
                pool.Add(item);
            }
        }

        public T Get()
        {
            if (this.currentConnections < this.maxConnections)
            {
                
                if (pool.TryTake(out T item))
                {
                    Interlocked.Increment(ref currentConnections);
                    return item;
                }
                else
                {
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }

        }

        public void Release(T obj)
        {
            obj.Reset();
            pool.Add(obj);
            Interlocked.Decrement(ref currentConnections);
        }
    }
}
