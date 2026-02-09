namespace GameApi.Services;

public class Result<T>
{
    public T? Data { get; private set; }
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }

    public static Result<T> Success()
        => new Result<T> { IsSuccess = true };

    public static Result<T> Success(T data)
        => new Result<T> { IsSuccess = true, Data = data };

    public static Result<T> Failure(string error)
        => new Result<T> { IsSuccess = false, Error = error };
}
