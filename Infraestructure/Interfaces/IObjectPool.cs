namespace Infraestructure.Interfaces
{
    public interface IObjectPool<T>
    {
        T Get();
        void Release(T obj);
    }
}
