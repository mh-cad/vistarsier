using System;

namespace CAPI.DAL.Abstraction
{
    public interface IRepository<in TEntity> : IDisposable 
        where TEntity : class
    {
        bool Add(TEntity entity);
        bool Update(TEntity entity);
        bool SaveChanges();
        bool Delete(TEntity entity);
    }
}