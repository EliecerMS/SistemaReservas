namespace SistemaReservas.Core
{

    // Generic container for a paged query result
    // typeparam name="T" is the type of the items in the page
    public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
}