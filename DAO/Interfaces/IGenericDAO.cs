using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DAO.Interfaces
{
    public interface IGenericDAO<T> where T : class, new()
    {
        T Create(T entity);
        T ReadById(int id);
        List<T> ReadAll();
        void Update(int id, T entity);
        void Delete(int id);
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate, int limit = 0);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
    }
}