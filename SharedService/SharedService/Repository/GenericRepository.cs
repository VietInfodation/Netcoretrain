using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SharedService.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace SharedService.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }
        public IQueryable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }
        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }
        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        //Map two item
        public T mapUpdateOject(T newItem, T updateItem)
        {
            var itemProperties = newItem.GetType().GetProperties();
            var newItemProperties = updateItem.GetType().GetProperties();
            List<Object> v = new List<Object>();

            // Update properties using reflection
            // Only set the value that is not null from  newItem
            foreach (var itemProperty in itemProperties)
            {
                var newItemProperty = newItemProperties.FirstOrDefault(p => p.Name == itemProperty.Name);
                if (newItemProperty != null)
                {
                    var itemValue = itemProperty.GetValue(newItem);
                    var newItemValue = newItemProperty.GetValue(updateItem);


                    if (itemValue == null)
                    {
                        itemProperty.SetValue(newItem, newItemValue);
                    }


                }
            }
            return newItem;
        }
        //**************OUTDATED
        public int UpdateSQLRaw(T item)
        {
            string elementUpdate = "";
            object elementId = "";
            PropertyInfo[] itemProperties = item.GetType().GetProperties(); // get all the properties of the item
            List<MySqlParameter> parameters = new List<MySqlParameter>();
            foreach (PropertyInfo property in itemProperties)
            {
                var value = property.GetValue(item);
                if (value == null) continue;

                if (property.Name.ToLower() == "id") // avoid setting id
                {
                    elementId = value;
                    continue;
                }
                elementUpdate += $"{property.Name}='{value}',"; // the "SET" part
                parameters.Add(new MySqlParameter($"@{property.Name}", value));
            }
            elementUpdate = elementUpdate.Substring(0, elementUpdate.Length - 1); // remove the last comma
            string sql =
               $"UPDATE {typeof(T).Name}s SET {elementUpdate} WHERE Id = @Id"; // SQL string
            parameters.Add(new MySqlParameter("@Id", elementId));

            return _context.Database.ExecuteSqlRaw(sql, parameters); //Run the raw SQL query
        }
    }
}
