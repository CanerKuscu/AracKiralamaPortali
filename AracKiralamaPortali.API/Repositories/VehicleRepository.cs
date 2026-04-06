using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle entity);
        void Update(Vehicle entity);
        void Delete(Vehicle entity);
        Task<int> SaveChangesAsync();
        IQueryable<Vehicle> GetQueryable();
    }

    public class VehicleRepository(AppDbContext context) : RepositoryBase<Vehicle>(context), IVehicleRepository;
}
