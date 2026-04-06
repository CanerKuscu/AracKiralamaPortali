using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IVehicleImageRepository
    {
        Task<VehicleImage?> GetByIdAsync(int id);
        Task AddAsync(VehicleImage entity);
        void Delete(VehicleImage entity);
        Task<int> SaveChangesAsync();
        IQueryable<VehicleImage> GetQueryable();
    }

    public class VehicleImageRepository(AppDbContext context) : RepositoryBase<VehicleImage>(context), IVehicleImageRepository;
}
