namespace HelpDeskLite.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null) =>
        new() { Success = false, Error = new ApiError { Message = message, Errors = errors } };
}

public class ApiError
{
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; init; }
}
