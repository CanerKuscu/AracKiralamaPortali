using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IBrandRepository
    {
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(int id);
        Task AddAsync(Brand entity);
        void Update(Brand entity);
        void Delete(Brand entity);
        Task<int> SaveChangesAsync();
        IQueryable<Brand> GetQueryable();
    }

    public class BrandRepository(AppDbContext context) : RepositoryBase<Brand>(context), IBrandRepository;
}
