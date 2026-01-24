using DonationManagementSystem.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _db;
        private readonly DbSet<T> _set;

        public Repository(DbContext db)
        {
            _db = db;
            _set = _db.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
            => await _set.FindAsync(id);

        public async Task<List<T>> GetAllAsync()
            => await _set.AsNoTracking().ToListAsync();

        public IQueryable<T> Query()
            => _set.AsQueryable();

        public async Task AddAsync(T entity)
            => await _set.AddAsync(entity);

        public void Update(T entity)
            => _set.Update(entity);

        public void Remove(T entity)
            => _set.Remove(entity);
    }
}