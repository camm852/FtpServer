namespace Infraestructure.Interfaces
{
    public interface IObjectFactory<T>
    {
        T CreateObject();
    }
}
