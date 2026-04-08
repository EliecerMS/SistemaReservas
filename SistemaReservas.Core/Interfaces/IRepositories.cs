using SistemaReservas.Core.Entities;

namespace SistemaReservas.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<Role?> GetRoleByNameAsync(string roleName);
    }

    public interface IRefreshTokenRepository
    {
        Task<int> CreateAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        /// <summary>Revokes a single token. If replacedBy is supplied, stores the rotation audit trail.</summary>
        Task RevokeAsync(int id, string? replacedBy = null);
        /// <summary>Revokes all active tokens for a user — called on logout.</summary>
        Task RevokeAllByUserIdAsync(int userId);
    }

    public interface IZoneRepository
    {

        // returns single page of active zones (pagination for future Zone addition s)

        Task<PagedResult<Zone>> GetAllAsync(int page = 1, int pageSize = 20);

        Task<Zone?> GetByIdAsync(int id);
        Task<IEnumerable<ZoneImage>> GetImagesByZoneIdAsync(int zoneId);
        Task<IEnumerable<EventType>> GetEventTypesByZoneIdAsync(int zoneId);
        Task<IEnumerable<EventType>> GetAllEventTypesAsync();
    }

    public interface IReservationRepository
    {
        // creates reservation usingg a stored procedure with UPDLOCK/ROWLOCK for concurrency control
        // throws SqlException with message 'zone unavialable' for scheduling conflict
        Task<int> CreateAsync(Reservation reservation);
        Task<Reservation?> GetByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
        // gets paged list of reservations optional "status filter" applied before pagingg
        Task<PagedResult<Reservation>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20);
        Task UpdateStatusAsync(int id, int statusId);
        Task<bool> HasConflictAsync(int zoneId, DateTime start, DateTime end, int? excludeReservationId = null);
    }
}
