using System.Net;

#nullable enable
namespace iBMSApp.Utility
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
