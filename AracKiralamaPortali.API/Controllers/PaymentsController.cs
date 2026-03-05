using AracKiralamaPortali.API.DTOs;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Reservation> _reservationRepository;

        public PaymentsController(
            IRepository<Payment> paymentRepository,
            IRepository<Reservation> reservationRepository)
        {
            _paymentRepository = paymentRepository;
            _reservationRepository = reservationRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentRepository.GetAllAsync();
            var dtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                ReservationId = p.ReservationId
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            var dto = new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                ReservationId = payment.ReservationId
            };
            return Ok(dto);
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetByReservation(int reservationId)
        {
            var payment = await _paymentRepository.GetAsync(p => p.ReservationId == reservationId);
            if (payment == null)
                return NotFound();

            var dto = new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                ReservationId = payment.ReservationId
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(dto.ReservationId);
            if (reservation == null)
                return NotFound(new { message = "Reservation not found." });

            var existingPayment = await _paymentRepository.AnyAsync(p => p.ReservationId == dto.ReservationId);
            if (existingPayment)
                return BadRequest(new { message = "Payment already exists for this reservation." });

            var payment = new Payment
            {
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                ReservationId = dto.ReservationId
            };

            await _paymentRepository.AddAsync(payment);

            reservation.Status = "Confirmed";
            _reservationRepository.Update(reservation);

            await _paymentRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, new PaymentDto
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                ReservationId = payment.ReservationId
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateDto dto)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = dto.Status;

            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync();

            return Ok(new { message = "Payment updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            _paymentRepository.Delete(payment);
            await _paymentRepository.SaveChangesAsync();

            return Ok(new { message = "Payment deleted successfully." });
        }
    }
}
