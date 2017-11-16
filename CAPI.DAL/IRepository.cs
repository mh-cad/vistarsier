using System;
using System.Linq;

namespace CAPI.DAL
{
    public interface IRepository<TEntity, in TKey> : IDisposable 
        where TEntity : class
    {
        bool Add(TEntity entity);
        TEntity Get(TKey id);
        IQueryable<TEntity> GetAll();
        bool Update(TEntity entity);
        bool SaveChanges();
        bool Delete(TEntity entity);
    }
}