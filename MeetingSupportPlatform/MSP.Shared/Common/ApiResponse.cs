namespace MSP.Shared.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public ApiResponse(bool success, string message, T? data, IEnumerable<string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors;
        }
        public static ApiResponse<T> SuccessResponse(T? Data, string message = "Request was successful")
        {
            return new ApiResponse<T>(true, message, Data);
        }

        public static ApiResponse<T> ErrorResponse(T? Data, string message = "Request was failed", IEnumerable<string>? errors = null)
        {
            return new ApiResponse<T>(false, message, Data, errors);
        }
    }
}
