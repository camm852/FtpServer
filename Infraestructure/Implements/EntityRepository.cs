using Domain.Entities;
using Infraestructure.Interfaces;

namespace Infraestructure.Implements
{
    public class EntityRepository<T> : IEntityRepository<T>
    {

        private List<T> list;


        public EntityRepository()
        {
            list = new List<T>();
        }

        public bool newEntity(T entity)
        {
            try
            {
                this.list.Add(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool deleteEntity(T entity)
        {
            try
            {
                if (getEntity(entity) != null)
                {
                    return this.list.Remove(entity);
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public T getEntity(T entity)
        {
            try
            {
                return this.list.FirstOrDefault(entity);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public List<T> getEntities()
        {
            return this.list;
        }


    }
}
