using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace DonationManagementSystem.Infrastructure.Services
{
    public class DonationCaseService : IDonationCaseService
    {
        private readonly ApplicationDbContext _db;

        public DonationCaseService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DonationCase?> GetByIdAsync(int id)
        {
            return await _db.DonationCases.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}