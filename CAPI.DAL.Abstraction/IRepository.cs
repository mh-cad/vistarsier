using System;

namespace CAPI.DAL.Abstraction
{
    public interface IRepository<in T> : IDisposable
        where T : class
    {
        bool Add(T entity);
        bool Update(T entity);
        bool SaveChanges();
        bool Delete(T entity);
    }
}