using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.DonationCases
{
    public interface IDonationCaseService
    {
        Task<DonationCase?> GetByIdAsync(int id);
        Task SaveAsync();
    }
}
