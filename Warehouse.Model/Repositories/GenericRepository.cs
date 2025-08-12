using Warehouse.Model;
using Warehouse.Domain.Inerfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Warehouse.Model.Repository
{
     internal class GenericRepository<T>:IGenericRepository<T> where T : class
     {
        private readonly ApplicationDbContext _storeContext;
        private DbSet<T> _set;

        public GenericRepository(ApplicationDbContext storeContext)
        {
            _storeContext = storeContext;
        }
        protected DbSet<T> Set
        {
            get { return _set ?? (_set = _storeContext.Set<T>()); }
        }
        public void DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            try
            {
                return await Set.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IReadOnlyList<T>> GetAllAsync(string includeProperties = "")
        {
            IQueryable<T> query = Set;

            try
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
           
                return await query.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<T> GetByIdAsync(int id)
        {
            try
            {
                return await Set.FindAsync((int) id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// not implimented
        /// </summary>
        /// <param name="entity"></param>
        public void UpdateAsync(T entity)
        {
            var xx = _storeContext.Attach<T>(entity);
            _storeContext.Entry(entity).State = EntityState.Modified;
        }
        ////Specification Pattern
        //public async Task<T> GetEntityWithSpec(ISpecifications<T> specification)
        //{
        //    return await ApplySpecification(specification).FirstOrDefaultAsync();
        //}


        //public async Task<IReadOnlyList<T>> ListAsync(ISpecifications<T> specification)
        //{
        //    return await ApplySpecification(specification).ToListAsync();
        //}
        //public async Task<int> CountAsync(ISpecifications<T> specifications)
        //{
        //    return await ApplySpecification(specifications).CountAsync();
        //}
        //private IQueryable<T> ApplySpecification(ISpecifications<T> specifications)
        //{
        //    return SpecificationEvaluatOr<T>.GetQuery(Set.AsQueryable(), specifications);
        //}

        public void Add(T entity)
        {
            _storeContext.Add<T>(entity);
        }

        public void Update(T entity)
        {
            var xx = _storeContext.Attach<T>(entity);
            _storeContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            Set.Remove(entity);

        }
       

        public bool Contains(Expression<Func<T, bool>> predicate)
        {
            return Count(predicate) > 0 ? true : false;
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return Set.Where(predicate).Count();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            Set.AddRange(entities);
        }

        //public Task<T> GeTWithSpec(ISpecifications<T> specification)
        //{
        //    throw new NotImplementedException();
        //}

        //public T GetByIdIncludeAsync(long id, string includeProperties = "", bool tracking = false)
        //{
        //    IQueryable<T> query = Set;
        //    foreach (var includeProperty in includeProperties.Split
        //        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        query = query.Include(includeProperty);
        //    }
        //    if (!tracking)
        //        return query.FirstOrDefault(s => s.Id == id);
        //    else
        //        return query.AsNoTracking().FirstOrDefault(s => s.Id == id);
        //}

        public async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "", int pagenum = 1, int pagesize = 50, bool tracking = false)
        {
            IQueryable<T> query = Set;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            if (pagesize > 0)
            {
                if (pagesize > 50)
                    pagesize = 50;
                if (pagenum < 1)
                    pagenum = 1;
                query.Skip((pagenum - 1) * 50).Take(pagesize);
            }

            if (orderBy != null)
            {
                if (!tracking)
                    return await orderBy(query).ToListAsync();
                else
                    return await orderBy(query).AsNoTracking().ToListAsync();
            }
            else
            {
                if (!tracking)
                    return await query.ToListAsync();
                else
                    return await query.AsNoTracking().ToListAsync();
            }
        }

        public async Task<List<T>> ExecuteSqlAsync(string sqlQuery)
        {
            return await _storeContext.Set<T>().FromSqlRaw(sqlQuery).ToListAsync();
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal?>> selector)
        {
            
            return (await Set.SumAsync(selector)) ?? 0;
        }

        /// <summary>
        /// get backup from all tables
        /// </summary>
        /// <returns></returns>
        public async Task<string> ExportDatabaseToJsonAsync()
        {
            var databaseData = new Dictionary<string, object>();

            // Get all DbSet properties from the DbContext
            var dbSets = _storeContext.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            foreach (var dbSetProperty in dbSets)
            {
                var dbSet = dbSetProperty.GetValue(_storeContext);
                var genericType = dbSetProperty.PropertyType.GetGenericArguments().First();

                // Use reflection to call ToListAsync() on the DbSet
                var toListAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethod(nameof(EntityFrameworkQueryableExtensions.ToListAsync), BindingFlags.Public | BindingFlags.Static)
                    ?.MakeGenericMethod(genericType);

                if (toListAsyncMethod != null)
                {
                    var task = (Task)toListAsyncMethod.Invoke(null, new object[] { dbSet, null });
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                    var tableData = resultProperty?.GetValue(task);

                    databaseData.Add(dbSetProperty.Name, tableData);
                }
            }


            // Serialize dictionary to JSON
            string jsonData = JsonSerializer.Serialize(databaseData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Define file path
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "database_backup.json");

            // Write JSON to file
            await File.WriteAllTextAsync(filePath, jsonData);

            return filePath; // Return the file path of the JSON file
        }

        public IQueryable<T> GetAllAsync_AsIqueryable()
        {
            return Set.AsNoTracking();
        }

    }
}
