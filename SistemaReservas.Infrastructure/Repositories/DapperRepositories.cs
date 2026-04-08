using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SistemaReservas.Core;
using SistemaReservas.Core.Entities;
using SistemaReservas.Core.Interfaces;
using System.Data;

namespace SistemaReservas.Infrastructure.Repositories
{
    public class DapperUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public DapperUserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"INSERT INTO Users (FullName, Email, PasswordHash, Phone, RoleId, CreatedAt, IsActive) 
                        VALUES (@FullName, @Email, @PasswordHash, @Phone, @RoleId, @CreatedAt, @IsActive); 
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT u.*, r.Name as RoleName 
                        FROM Users u 
                        JOIN Roles r ON u.RoleId = r.Id 
                        WHERE u.Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT u.*, r.Name as RoleName 
                        FROM Users u 
                        JOIN Roles r ON u.RoleId = r.Id 
                        WHERE u.Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Roles WHERE Name = @Name";
            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = roleName });
        }
    }

    public class DapperZoneRepository : IZoneRepository
    {
        private readonly string _connectionString;

        public DapperZoneRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<PagedResult<Zone>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            using var connection = new SqlConnection(_connectionString);
            var countSql = "SELECT COUNT(*) FROM Zones WHERE IsActive = 1";
            var dataSql = @"SELECT * FROM Zones
                            WHERE IsActive = 1
                            ORDER BY Name -- arbitrary order, change if needed
                            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            using var multi = await connection.QueryMultipleAsync(countSql + ";" + dataSql, new { Offset = offset, PageSize = pageSize });
            var total = await multi.ReadSingleAsync<int>();
            var items = await multi.ReadAsync<Zone>();
            return new PagedResult<Zone>(items, total, page, pageSize);
        }

        public async Task<Zone?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Zones WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Zone>(sql, new { Id = id });
        }

        public async Task<IEnumerable<ZoneImage>> GetImagesByZoneIdAsync(int zoneId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM ZoneImages WHERE ZoneId = @ZoneId ORDER BY IsPrimary DESC";
            return await connection.QueryAsync<ZoneImage>(sql, new { ZoneId = zoneId });
        }

        public async Task<IEnumerable<EventType>> GetEventTypesByZoneIdAsync(int zoneId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT et.* FROM EventTypes et 
                        JOIN ZoneEventTypes zet ON et.Id = zet.EventTypeId 
                        WHERE zet.ZoneId = @ZoneId";
            return await connection.QueryAsync<EventType>(sql, new { ZoneId = zoneId });
        }

        public async Task<IEnumerable<EventType>> GetAllEventTypesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM EventTypes";
            return await connection.QueryAsync<EventType>(sql);
        }
    }

    public class DapperReservationRepository : IReservationRepository
    {
        private readonly string _connectionString;

        public DapperReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(Reservation reservation)
        {
            using var connection = new SqlConnection(_connectionString);
            // uses stored procedure with UPDLOCK/ROWLOCK for concurrency control
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", reservation.UserId);
            parameters.Add("@ZoneId", reservation.ZoneId);
            parameters.Add("@EventTypeId", reservation.EventTypeId);
            parameters.Add("@StartDateTime", reservation.StartDateTime);
            parameters.Add("@EndDateTime", reservation.EndDateTime);
            parameters.Add("@GuestCount", reservation.GuestCount);
            parameters.Add("@TotalPrice", reservation.TotalPrice);
            parameters.Add("@Notes", reservation.Notes);

            var newId = await connection.ExecuteScalarAsync<int>(
                "sp_CreateReservation",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return newId;
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT r.*, z.Name as ZoneName, et.Name as EventTypeName, 
                            rs.Name as StatusName, u.FullName as UserFullName
                        FROM Reservations r
                        JOIN Zones z ON r.ZoneId = z.Id
                        JOIN EventTypes et ON r.EventTypeId = et.Id
                        JOIN ReservationStatuses rs ON r.StatusId = rs.Id
                        JOIN Users u ON r.UserId = u.Id
                        WHERE r.Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Reservation>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT r.*, z.Name as ZoneName, et.Name as EventTypeName, 
                            rs.Name as StatusName, u.FullName as UserFullName
                        FROM Reservations r
                        JOIN Zones z ON r.ZoneId = z.Id
                        JOIN EventTypes et ON r.EventTypeId = et.Id
                        JOIN ReservationStatuses rs ON r.StatusId = rs.Id
                        JOIN Users u ON r.UserId = u.Id
                        WHERE r.UserId = @UserId
                        ORDER BY r.StartDateTime DESC";
            return await connection.QueryAsync<Reservation>(sql, new { UserId = userId });
        }

        public async Task<PagedResult<Reservation>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20)
        {
            using var connection = new SqlConnection(_connectionString);
            var whereClause = statusId.HasValue ? "WHERE r.StatusId = @StatusId" : string.Empty;
            var countSql = $@"SELECT COUNT(*)
                               FROM Reservations r
                               {whereClause}";
            var dataSql = $@"SELECT r.*, z.Name as ZoneName, et.Name as EventTypeName, 
                            rs.Name as StatusName, u.FullName as UserFullName
                        FROM Reservations r
                        JOIN Zones z ON r.ZoneId = z.Id
                        JOIN EventTypes et ON r.EventTypeId = et.Id
                        JOIN ReservationStatuses rs ON r.StatusId = rs.Id
                        JOIN Users u ON r.UserId = u.Id
                        {whereClause}
                        ORDER BY r.StartDateTime DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var offset = (page - 1) * pageSize;
            using var multi = await connection.QueryMultipleAsync(countSql + ";" + dataSql, new { StatusId = statusId, Offset = offset, PageSize = pageSize });
            var total = await multi.ReadSingleAsync<int>();
            var items = await multi.ReadAsync<Reservation>();
            return new PagedResult<Reservation>(items, total, page, pageSize);
        }

        public async Task UpdateStatusAsync(int id, int statusId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Reservations SET StatusId = @StatusId, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, StatusId = statusId });
        }

        public async Task<bool> HasConflictAsync(int zoneId, DateTime start, DateTime end, int? excludeReservationId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT CASE WHEN EXISTS (
                            SELECT 1 FROM Reservations 
                            WHERE ZoneId = @ZoneId 
                              AND StatusId IN (1, 2)
                              AND NOT (@EndDateTime <= StartDateTime OR @StartDateTime >= EndDateTime)
                              AND (@ExcludeId IS NULL OR Id != @ExcludeId)
                        ) THEN 1 ELSE 0 END";
            return await connection.ExecuteScalarAsync<bool>(sql,
                new { ZoneId = zoneId, StartDateTime = start, EndDateTime = end, ExcludeId = excludeReservationId });
        }
    }

    public class DapperRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public DapperRefreshTokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateAsync(RefreshToken token)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, IsRevoked, CreatedAt)
                        VALUES (@UserId, @Token, @ExpiresAt, 0, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                token.UserId,
                token.Token,
                token.ExpiresAt,
                token.CreatedAt
            });
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM RefreshTokens WHERE Token = @Token";
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeAsync(int id, string? replacedBy = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"UPDATE RefreshTokens
                        SET IsRevoked = 1, ReplacedBy = @ReplacedBy
                        WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, ReplacedBy = replacedBy });
        }

        public async Task RevokeAllByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0";
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }
    }
}
