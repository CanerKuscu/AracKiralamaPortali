using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    // Araç görselleri için gereken CRUD imzalarý.
    // Uygulamalar `RepositoryBase<VehicleImage>` üzerinden saðlanýr.
    public interface IVehicleImageRepository
    {
        Task<VehicleImage?> GetByIdAsync(int id);
        Task AddAsync(VehicleImage entity);
        void Delete(VehicleImage entity);
        Task<int> SaveChangesAsync();
        IQueryable<VehicleImage> GetQueryable();
    }

    // Metot gövdeleri `RepositoryBase` içinde olduðundan burada tekrar yazýlmadý.
    public class VehicleImageRepository(AppDbContext context) : RepositoryBase<VehicleImage>(context), IVehicleImageRepository;
}
