using Warehouse.Model;
using Warehouse.Domain;
using Warehouse.Domain.Inerfaces;
using Warehouse.Model.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private Hashtable _repositories;
        
        public UnitOfWork(ApplicationDbContext MyDbContext)
        {
            _dbContext = MyDbContext;
        }
        public async Task<int> Complete()
        {
            return await _dbContext.SaveChangesAsync();
        }
        public DbContext GetDbContext()
        {
            return _dbContext;
        }
        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public IGenericRepository<TEntity> repository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();
            var Type = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(Type))
            {
                var repositiryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositiryType.MakeGenericType(typeof(TEntity)), _dbContext);
                _repositories.Add(Type, repositoryInstance);
            }
            return (IGenericRepository<TEntity>)_repositories[Type];
        }


     

    }
}
