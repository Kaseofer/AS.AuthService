namespace AgendaSalud.AuthService.Api.Common
{
    public class ResponseApi<T> 
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; internal set; }
        public T? Data { get; internal set; }
        public string ErrorCode { get; internal set; }
    }
}
