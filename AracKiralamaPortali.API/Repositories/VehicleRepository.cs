using System.Linq.Expressions;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    // Araç için gereken CRUD imzalarý.
    // Uygulamalar `RepositoryBase<Vehicle>` üzerinden saðlanýr.
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<IEnumerable<Vehicle>> GetAllAsync(Expression<Func<Vehicle, bool>> predicate);
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle entity);
        void Update(Vehicle entity);
        void Delete(Vehicle entity);
        Task<int> SaveChangesAsync();
        IQueryable<Vehicle> GetQueryable();
    }

    // Metot gövdeleri `RepositoryBase` içinde olduðundan burada tekrar yazýlmadý.
    public class VehicleRepository(AppDbContext context) : RepositoryBase<Vehicle>(context), IVehicleRepository;
}
