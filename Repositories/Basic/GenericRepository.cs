using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data;

namespace Repositories.Basic
{
    public interface IGenericRepository<T> where T : class
    {
        // Synchronous methods
        List<T> GetAll();
        T GetById(int id);
        T GetById(string code);
        T GetById(Guid code);
        void Create(T entity);
        void Update(T entity);
        bool Remove(T entity);
        int Save();

        // Asynchronous methods
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> GetByIdAsync(string code);
        Task<T> GetByIdAsync(Guid code);
        Task<int> CreateAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<bool> RemoveAsync(T entity);
        Task<int> SaveAsync();

        // Prepare methods for batch operations (without immediate save)
        void PrepareCreate(T entity);
        void PrepareUpdate(T entity);
        void PrepareRemove(T entity);
        // Advanced querying with filtering, pagination, and sorting
        Task<IEnumerable<T>> GetWithAdvancedQuery(
            Expression<Func<T, bool>>? filter = null,
            int page = 1,
            int pageSize = 10,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? searchField = null,
            string? searchValue = null);
        Task<int> CountWithFilter(Expression<Func<T, bool>>? filter = null, string? searchField = null, string? searchValue = null);
    }
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected VLivingDbContext _context;

        public GenericRepository()
        {
            _context ??= new VLivingDbContext();
        }

        public GenericRepository(VLivingDbContext context)
        {
            _context = context;
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public void Create(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        public async Task<int> CreateAsync(T entity)
        {
            _context.Add(entity);
            return await _context.SaveChangesAsync();
        }
        public void Update(T entity)
        {
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        public T GetById(Guid code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(Guid code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        public void PrepareCreate(T entity)
        {
            _context.Add(entity);
        }

        public void PrepareUpdate(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }

        public void PrepareRemove(T entity)
        {
            _context.Remove(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> GetWithAdvancedQuery(
        Expression<Func<T, bool>>? filter = null,
        int page = 1,
        int pageSize = 10,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? searchField = null,
        string? searchValue = null)
        {
            IQueryable<T> query = _context.Set<T>();

            // Apply base filter
            if (filter != null)
                query = query.Where(filter);

            // Apply dynamic search filter
            var searchExpression = BuildSearchExpression(searchField, searchValue);
            if (searchExpression != null)
                query = query.Where(searchExpression);

            if (orderBy != null)
                query = orderBy(query);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<int> CountWithFilter(Expression<Func<T, bool>>? filter = null, string? searchField = null, string? searchValue = null)
        {
            IQueryable<T> query = _context.Set<T>();

            // Apply base filter
            if (filter != null)
                query = query.Where(filter);

            // Apply dynamic search filter
            var searchExpression = BuildSearchExpression(searchField, searchValue);
            if (searchExpression != null)
                query = query.Where(searchExpression);

            return await query.CountAsync();
        }

        /// <summary>
        /// Build dynamic search expression based on searchField and searchValue
        /// Supports: string (Contains), int, decimal, double, bool
        /// </summary>
        protected Expression<Func<T, bool>>? BuildSearchExpression(string? searchField, string? searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchField) || string.IsNullOrWhiteSpace(searchValue))
                return null;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(searchField, 
                System.Reflection.BindingFlags.IgnoreCase | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance);

            if (property == null)
                return null;

            var propertyAccess = Expression.Property(parameter, property);
            var propertyType = property.PropertyType;

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            Expression? comparison = null;

            try
            {
                if (underlyingType == typeof(string))
                {
                    // String: Contains (case-insensitive)
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    
                    if (toLowerMethod != null && containsMethod != null)
                    {
                        var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
                        var valueConstant = Expression.Constant(searchValue.ToLower());
                        comparison = Expression.Call(propertyToLower, containsMethod, valueConstant);
                    }
                }
                else if (underlyingType == typeof(int))
                {
                    // Int: Equals
                    if (int.TryParse(searchValue, out int intValue))
                    {
                        var valueConstant = Expression.Constant(intValue, underlyingType);
                        
                        // Handle nullable
                        if (propertyType != underlyingType)
                        {
                            var hasValue = Expression.Property(propertyAccess, "HasValue");
                            var value = Expression.Property(propertyAccess, "Value");
                            var equals = Expression.Equal(value, valueConstant);
                            comparison = Expression.AndAlso(hasValue, equals);
                        }
                        else
                        {
                            comparison = Expression.Equal(propertyAccess, valueConstant);
                        }
                    }
                }
                else if (underlyingType == typeof(decimal))
                {
                    // Decimal: Equals
                    if (decimal.TryParse(searchValue, out decimal decimalValue))
                    {
                        var valueConstant = Expression.Constant(decimalValue, underlyingType);
                        
                        // Handle nullable
                        if (propertyType != underlyingType)
                        {
                            var hasValue = Expression.Property(propertyAccess, "HasValue");
                            var value = Expression.Property(propertyAccess, "Value");
                            var equals = Expression.Equal(value, valueConstant);
                            comparison = Expression.AndAlso(hasValue, equals);
                        }
                        else
                        {
                            comparison = Expression.Equal(propertyAccess, valueConstant);
                        }
                    }
                }
                else if (underlyingType == typeof(double))
                {
                    // Double: Equals
                    if (double.TryParse(searchValue, out double doubleValue))
                    {
                        var valueConstant = Expression.Constant(doubleValue, underlyingType);
                        
                        // Handle nullable
                        if (propertyType != underlyingType)
                        {
                            var hasValue = Expression.Property(propertyAccess, "HasValue");
                            var value = Expression.Property(propertyAccess, "Value");
                            var equals = Expression.Equal(value, valueConstant);
                            comparison = Expression.AndAlso(hasValue, equals);
                        }
                        else
                        {
                            comparison = Expression.Equal(propertyAccess, valueConstant);
                        }
                    }
                }
                else if (underlyingType == typeof(bool))
                {
                    // Bool: Equals (accepts: true/false, 1/0, yes/no)
                    bool boolValue;
                    if (bool.TryParse(searchValue, out boolValue) ||
                        (searchValue == "1" && (boolValue = true)) ||
                        (searchValue == "0" && (boolValue = false)) ||
                        (searchValue.Equals("yes", StringComparison.OrdinalIgnoreCase) && (boolValue = true)) ||
                        (searchValue.Equals("no", StringComparison.OrdinalIgnoreCase) && (boolValue = false)))
                    {
                        var valueConstant = Expression.Constant(boolValue, underlyingType);
                        
                        // Handle nullable
                        if (propertyType != underlyingType)
                        {
                            var hasValue = Expression.Property(propertyAccess, "HasValue");
                            var value = Expression.Property(propertyAccess, "Value");
                            var equals = Expression.Equal(value, valueConstant);
                            comparison = Expression.AndAlso(hasValue, equals);
                        }
                        else
                        {
                            comparison = Expression.Equal(propertyAccess, valueConstant);
                        }
                    }
                }
            }
            catch
            {
                // If any error occurs, return null (no search applied)
                return null;
            }

            if (comparison == null)
                return null;

            return Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }

    }
}
