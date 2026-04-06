using System.Linq.Expressions;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review entity);
        void Delete(Review entity);
        Task<bool> AnyAsync(Expression<Func<Review, bool>> predicate);
        Task<int> SaveChangesAsync();
        IQueryable<Review> GetQueryable();
    }

    public class ReviewRepository(AppDbContext context) : RepositoryBase<Review>(context), IReviewRepository;
}
