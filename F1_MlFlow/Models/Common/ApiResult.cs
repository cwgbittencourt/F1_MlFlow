namespace F1_MlFlow.Models.Common;

public sealed class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }

    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T> { IsSuccess = true, Data = data };
    }

    public static ApiResult<T> Failure(string message)
    {
        return new ApiResult<T> { IsSuccess = false, ErrorMessage = message };
    }
}
