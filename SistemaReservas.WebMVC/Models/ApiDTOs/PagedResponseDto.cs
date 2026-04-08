namespace SistemaReservas.WebMVC.Models.ApiDTOs
{
    public class PagedResponseDto<T>
    {
        // Coincide con el campo "items" del JSON
        public required IEnumerable<T> Items { get; init; } = [];

        // Coinciden con los metadatos del JSON
        public required int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
