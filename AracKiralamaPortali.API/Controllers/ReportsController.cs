using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Vehicle> _vehicleRepository;

        public ReportsController(
            IRepository<Reservation> reservationRepository,
            IRepository<Payment> paymentRepository,
            IRepository<Vehicle> vehicleRepository)
        {
            _reservationRepository = reservationRepository;
            _paymentRepository = paymentRepository;
            _vehicleRepository = vehicleRepository;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] int? year)
        {
            var y = year ?? DateTime.Now.Year;
            var payments = await _paymentRepository.GetQueryable()
                .Where(p => p.PaymentDate.Year == y && p.Status == "Completed")
                .ToListAsync();

            var monthly = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Total = payments.Where(p => p.PaymentDate.Month == m).Sum(p => p.Amount)
            });

            return Ok(new { year = y, monthlyRevenue = monthly, grandTotal = payments.Sum(p => p.Amount) });
        }

        [HttpGet("popular-vehicles")]
        public async Task<IActionResult> GetPopularVehicles()
        {
            var reservations = await _reservationRepository.GetQueryable()
                .Include(r => r.Vehicle).ThenInclude(v => v.Brand)
                .Where(r => r.Status != "Cancelled")
                .ToListAsync();

            var popular = reservations.GroupBy(r => r.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    Plate = g.First().Vehicle.Plate,
                    Brand = g.First().Vehicle.Brand.Name,
                    Model = g.First().Vehicle.Model,
                    RentalCount = g.Count(),
                    TotalRevenue = g.Sum(r => r.TotalPrice)
                })
                .OrderByDescending(x => x.RentalCount)
                .Take(10);

            return Ok(popular);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalVehicles = await _vehicleRepository.GetQueryable().CountAsync();
            var availableVehicles = await _vehicleRepository.GetQueryable().CountAsync(v => v.VehicleStatus == "Available");
            var totalReservations = await _reservationRepository.GetQueryable().CountAsync();
            var activeReservations = await _reservationRepository.GetQueryable().CountAsync(r => r.Status == "Confirmed");
            var totalRevenue = await _paymentRepository.GetQueryable().Where(p => p.Status == "Completed").SumAsync(p => p.Amount);

            return Ok(new { totalVehicles, availableVehicles, totalReservations, activeReservations, totalRevenue });
        }
    }
}
