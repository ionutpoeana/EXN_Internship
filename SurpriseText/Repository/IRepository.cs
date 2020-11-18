using System;
using System.Collections.Generic;

namespace SurpriseText
{
    public interface IRepository<T>
    {
        T Get(Func<T, bool> func);
        IEnumerable<T> GetAll();
        void Delete(T entity);
        void Update(T entity);
        void Add(T entity);
    }
}
