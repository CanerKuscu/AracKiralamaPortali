using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IAdditionalServiceRepository
    {
        Task<IEnumerable<AdditionalService>> GetAllAsync();
        Task<AdditionalService?> GetByIdAsync(int id);
        Task AddAsync(AdditionalService entity);
        void Update(AdditionalService entity);
        void Delete(AdditionalService entity);
        Task<int> SaveChangesAsync();
    }

    public class AdditionalServiceRepository(AppDbContext context) : RepositoryBase<AdditionalService>(context), IAdditionalServiceRepository;
}
