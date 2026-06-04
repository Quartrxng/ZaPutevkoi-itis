using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MyORMLibrary;
using DAO.Interfaces;

namespace DAO
{
    public class GenericDAO<T> : IGenericDAO<T> where T : class, new()
    {
        protected readonly ORMContext _context;

        public GenericDAO(string connectionString)
        {
            _context = new ORMContext(connectionString);
        }

        public T Create(T entity)
        {
            return _context.Create(entity);
        }

        public T ReadById(int id)
        {
            return _context.ReadById<T>(id);
        }

        public List<T> ReadAll()
        {
            return _context.ReadByAll<T>();
        }

        public void Update(int id, T entity)
        {
            _context.Update(id, entity);
        }

        public void Delete(int id)
        {
            _context.Delete<T>(id);
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate, int limit = 0)
        {
            return _context.Where(predicate, limit);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _context.FirstOrDefault(predicate);
        }
    }
}