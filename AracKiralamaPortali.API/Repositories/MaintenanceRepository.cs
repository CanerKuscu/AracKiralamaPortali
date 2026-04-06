using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IMaintenanceRepository
    {
        IQueryable<Maintenance> GetQueryable();
        Task<Maintenance?> GetByIdAsync(int id);
        Task AddAsync(Maintenance entity);
        void Update(Maintenance entity);
        void Delete(Maintenance entity);
        Task<int> SaveChangesAsync();
    }

    public class MaintenanceRepository(AppDbContext context) : RepositoryBase<Maintenance>(context), IMaintenanceRepository;
}
