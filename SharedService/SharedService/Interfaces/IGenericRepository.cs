using System.Linq.Expressions;

namespace SharedService.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        T GetById(int id);
        IQueryable<T> GetAll();
        IQueryable<T> Find(Expression<Func<T, bool>> expression);
        void Update(T entity);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
        void UpdateRange(IEnumerable<T> entities);

        T mapUpdateOject(T item, T newItem);
        int UpdateSQLRaw(T item);
    }
}
