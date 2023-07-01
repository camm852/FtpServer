using Domain.Entities;

namespace Infraestructure.Interfaces
{
    public interface IEntityRepository<T>
    {
        bool newEntity(T entity);
        bool deleteEntity(T entity);
        List<T> getEntities();
    }
}
    