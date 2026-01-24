using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();

        IQueryable<T> Query(); // allow Includes, Where, etc.

        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}