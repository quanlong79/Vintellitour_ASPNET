namespace Vintellitour_Framework.Models.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
