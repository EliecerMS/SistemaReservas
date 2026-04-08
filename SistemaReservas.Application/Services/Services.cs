using Microsoft.Extensions.Configuration;
using SistemaReservas.Application.DTOs;
using SistemaReservas.Application.Interfaces;
using SistemaReservas.Core;
using SistemaReservas.Core.Entities;
using SistemaReservas.Core.Interfaces;

namespace SistemaReservas.Application.Services
{

    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task RegisterAsync(RegisterDto dto);
        /// <summary>Validates a refresh token, rotates it, and returns a fresh access JWT + new refresh token.</summary>
        Task<LoginResponseDto> RefreshAsync(string refreshToken);
        /// <summary>Revokes the given refresh token (used on logout).</summary>
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

    public interface IZoneService
    {
        Task<PagedResult<ZoneResponseDto>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<ZoneResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<EventTypeDto>> GetAllEventTypesAsync();
    }

    public interface IReservationService
    {
        Task<int> CreateAsync(int userId, CreateReservationDto dto);
        Task CancelAsync(int userId, int reservationId, bool isAdmin);
        Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(int userId);
        Task<PagedResult<ReservationResponseDto>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20);
        Task ConfirmAsync(int reservationId);
    }


    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("La cuenta está desactivada.");

            var accessToken  = _tokenGenerator.GenerateToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new LoginResponseDto
            {
                Token        = accessToken,
                RefreshToken = refreshToken,
                FullName     = user.FullName,
                Email        = user.Email,
                Role         = user.RoleName
            };
        }

        public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken)
                ?? throw new UnauthorizedAccessException("Token de refresco inválido.");

            if (!stored.IsActive)
                throw new UnauthorizedAccessException("El token de refresco ha expirado o fue revocado.");

            var user = await _userRepository.GetByIdAsync(stored.UserId)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("La cuenta está desactivada.");

            // Rotate: revoke old token and issue a brand-new one
            var newRawToken  = _tokenGenerator.GenerateRefreshToken();
            await _refreshTokenRepository.RevokeAsync(stored.Id, replacedBy: newRawToken);
            var expiryDays   = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            var newRefreshToken = new RefreshToken
            {
                UserId    = user.Id,
                Token     = newRawToken,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.CreateAsync(newRefreshToken);

            var accessToken = _tokenGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                Token        = accessToken,
                RefreshToken = newRawToken,
                FullName     = user.FullName,
                Email        = user.Email,
                Role         = user.RoleName
            };
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (stored != null && !stored.IsRevoked)
                await _refreshTokenRepository.RevokeAsync(stored.Id);
        }

        // ── private helpers ──────────────────────────────────────────────

        private async Task<string> CreateRefreshTokenAsync(int userId)
        {
            var raw        = _tokenGenerator.GenerateRefreshToken();
            var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
            var entity     = new RefreshToken
            {
                UserId    = userId,
                Token     = raw,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.CreateAsync(entity);
            return raw;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Ya existe un usuario con ese correo electrónico.");

            var role = await _userRepository.GetRoleByNameAsync("Cliente");
            if (role == null) throw new Exception("Rol 'Cliente' no encontrado en la base de datos.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Phone = dto.Phone,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);
        }
    }


    public class ZoneService : IZoneService
    {
        private readonly IZoneRepository _zoneRepository;

        public ZoneService(IZoneRepository zoneRepository)
        {
            _zoneRepository = zoneRepository;
        }

        public async Task<PagedResult<ZoneResponseDto>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            var pagedZones = await _zoneRepository.GetAllAsync(page, pageSize);
            var resultItems = new List<ZoneResponseDto>();

            foreach (var zone in pagedZones.Items)
            {
                var images = await _zoneRepository.GetImagesByZoneIdAsync(zone.Id);
                var eventTypes = await _zoneRepository.GetEventTypesByZoneIdAsync(zone.Id);

                resultItems.Add(new ZoneResponseDto
                {
                    Id = zone.Id,
                    Name = zone.Name,
                    Description = zone.Description,
                    Capacity = zone.Capacity,
                    PricePerHour = zone.PricePerHour,
                    IsActive = zone.IsActive,
                    Images = images.Select(i => new ZoneImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary
                    }).ToList(),
                    EventTypes = eventTypes.Select(e => new EventTypeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Description = e.Description
                    }).ToList()
                });
            }

            return new PagedResult<ZoneResponseDto>(resultItems, pagedZones.TotalCount, pagedZones.Page, pagedZones.PageSize);
        }

        public async Task<ZoneResponseDto?> GetByIdAsync(int id)
        {
            var zone = await _zoneRepository.GetByIdAsync(id);
            if (zone == null) return null;

            var images = await _zoneRepository.GetImagesByZoneIdAsync(zone.Id);
            var eventTypes = await _zoneRepository.GetEventTypesByZoneIdAsync(zone.Id);

            return new ZoneResponseDto
            {
                Id = zone.Id,
                Name = zone.Name,
                Description = zone.Description,
                Capacity = zone.Capacity,
                PricePerHour = zone.PricePerHour,
                IsActive = zone.IsActive,
                Images = images.Select(i => new ZoneImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                }).ToList(),
                EventTypes = eventTypes.Select(e => new EventTypeDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description
                }).ToList()
            };
        }

        public async Task<IEnumerable<EventTypeDto>> GetAllEventTypesAsync()
        {
            var eventTypes = await _zoneRepository.GetAllEventTypesAsync();
            return eventTypes.Select(e => new EventTypeDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            });
        }
    }


    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IZoneRepository _zoneRepository;

        public ReservationService(IReservationRepository reservationRepository, IZoneRepository zoneRepository)
        {
            _reservationRepository = reservationRepository;
            _zoneRepository = zoneRepository;
        }

        public async Task<int> CreateAsync(int userId, CreateReservationDto dto)
        {
            // validate dates
            if (dto.StartDateTime >= dto.EndDateTime)
                throw new ArgumentException("La hora de inicio debe ser anterior a la hora de fin.");

            if (dto.StartDateTime < DateTime.UtcNow)
                throw new ArgumentException("No se puede reservar en una fecha pasada.");

            // validate zone exists and is active
            var zone = await _zoneRepository.GetByIdAsync(dto.ZoneId);
            if (zone == null)
                throw new KeyNotFoundException("La zona solicitada no existe.");
            if (!zone.IsActive)
                throw new InvalidOperationException("La zona no está disponible actualmente.");

            // validate capacity
            if (dto.GuestCount > zone.Capacity)
                throw new InvalidOperationException(
                    $"El número de invitados ({dto.GuestCount}) supera la capacidad de la zona ({zone.Capacity}).");

            // calculate total price
            var hours = (decimal)(dto.EndDateTime - dto.StartDateTime).TotalHours;
            var totalPrice = zone.PricePerHour * hours;

            // check availability (double-check before hitting Store Procedure)
            var hasConflict = await _reservationRepository.HasConflictAsync(dto.ZoneId, dto.StartDateTime, dto.EndDateTime);
            if (hasConflict)
                throw new InvalidOperationException("La zona no está disponible en ese horario. Por favor seleccione otro horario.");

            var reservation = new Reservation
            {
                UserId = userId,
                ZoneId = dto.ZoneId,
                EventTypeId = dto.EventTypeId,
                StatusId = 1,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                GuestCount = dto.GuestCount,
                TotalPrice = totalPrice,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            // SP does final concurrency check with UPDLOCK
            return await _reservationRepository.CreateAsync(reservation);
        }

        public async Task CancelAsync(int userId, int reservationId, bool isAdmin)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("La reserva no existe.");

            // only owner or admin can cancel
            if (!isAdmin && reservation.UserId != userId)
                throw new UnauthorizedAccessException("No tiene permisos para cancelar esta reserva.");

            // check if already cancelled or completed
            if (reservation.StatusId == 3)
                throw new InvalidOperationException("La reserva ya fue cancelada.");
            if (reservation.StatusId == 4)
                throw new InvalidOperationException("No se puede cancelar una reserva completada.");

            // 48-hour cancellation rule (skip for admin)
            if (!isAdmin && reservation.StartDateTime.Subtract(DateTime.UtcNow).TotalHours < 48)
                throw new InvalidOperationException("No se puede cancelar con menos de 48 horas de anticipación.");

            await _reservationRepository.UpdateStatusAsync(reservationId, 3);
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(int userId)
        {
            var reservations = await _reservationRepository.GetByUserIdAsync(userId);
            return reservations.Select(MapToDto);
        }

        public async Task<PagedResult<ReservationResponseDto>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20)
        {
            var pagedReservations = await _reservationRepository.GetAllAsync(statusId, page, pageSize);
            var items = pagedReservations.Items.Select(MapToDto);
            return new PagedResult<ReservationResponseDto>(items, pagedReservations.TotalCount, pagedReservations.Page, pagedReservations.PageSize);
        }

        public async Task ConfirmAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException("La reserva no existe.");

            if (reservation.StatusId != 1)
                throw new InvalidOperationException("Solo se pueden confirmar reservas pendientes.");

            await _reservationRepository.UpdateStatusAsync(reservationId, 2);
        }

        private static ReservationResponseDto MapToDto(Reservation r) => new()
        {
            Id = r.Id,
            ZoneName = r.ZoneName,
            EventTypeName = r.EventTypeName,
            StatusName = r.StatusName,
            StartDateTime = r.StartDateTime,
            EndDateTime = r.EndDateTime,
            GuestCount = r.GuestCount,
            TotalPrice = r.TotalPrice,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt,
            UserFullName = r.UserFullName
        };
    }
}
