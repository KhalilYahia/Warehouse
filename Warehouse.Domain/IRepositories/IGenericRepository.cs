using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Domain.Inerfaces
{
    public interface IGenericRepository<T> where T : class 
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync(string includeProperties = "");
        //T GetByIdIncludeAsync(long id, string includeProperties = "", bool tracking = false);

        //Task<T> GeTWithSpec(ISpecifications<T> specification);
        //Task<IReadOnlyList<T>> ListAsync(ISpecifications<T> specification);
        //Task<int> CountAsync(ISpecifications<T> specifications);
        void DeleteAsync(T entity);
        void UpdateAsync(T entity);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool Contains(Expression<Func<T, bool>> predicate);
        Task<string> ExportDatabaseToJsonAsync();
        int Count(Expression<Func<T, bool>> predicate);
        void AddRange(IEnumerable<T> entities);
        Task<IEnumerable<T>> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "", int pagesize = 1, int pagenum = 50, bool tracking = false);

        Task<List<T>> ExecuteSqlAsync(string sqlQuery);
        Task<decimal> SumAsync(Expression<Func<T, decimal?>> selector);

        IQueryable<T> GetAllAsync_AsIqueryable(); 
    }
    
}
