namespace APIGateway.Models
{
    /// <summary>
    /// Modelo estandarizado que el API Gateway devuelve al cliente cuando ocurre un error.
    /// </summary>
    public class ApiErrorResponse
    {
        public bool Success { get; set; } = false;

        public int Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public IReadOnlyCollection<string> Errors { get; set; } = Array.Empty<string>();

        public string? Details { get; set; }

        public string TraceId { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
