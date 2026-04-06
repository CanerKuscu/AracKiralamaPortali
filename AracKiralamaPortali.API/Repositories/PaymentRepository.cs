using System.Linq.Expressions;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetAsync(Expression<Func<Payment, bool>> predicate);
        Task AddAsync(Payment entity);
        void Update(Payment entity);
        void Delete(Payment entity);
        Task<bool> AnyAsync(Expression<Func<Payment, bool>> predicate);
        Task<int> SaveChangesAsync();
        IQueryable<Payment> GetQueryable();
    }

    public class PaymentRepository(AppDbContext context) : RepositoryBase<Payment>(context), IPaymentRepository;
}
