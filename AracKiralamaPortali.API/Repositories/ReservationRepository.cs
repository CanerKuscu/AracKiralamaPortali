using System.Linq.Expressions;
using AracKiralamaPortali.API.Data;
using AracKiralamaPortali.API.Models;

namespace AracKiralamaPortali.API.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(int id);
        Task AddAsync(Reservation entity);
        void Update(Reservation entity);
        void Delete(Reservation entity);
        Task<bool> AnyAsync(Expression<Func<Reservation, bool>> predicate);
        Task<int> SaveChangesAsync();
        IQueryable<Reservation> GetQueryable();
    }

    public class ReservationRepository(AppDbContext context) : RepositoryBase<Reservation>(context), IReservationRepository;
}
