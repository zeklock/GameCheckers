namespace GameApi.Services;

public class Result<T> : Result
{
    public T? Data { get; private set; }

    public static Result<T> Success(T data)
        => new Result<T> { IsSuccess = true, Data = data };

    public static new Result<T> Failure(string error)
        => new Result<T> { IsSuccess = false, Error = error };
}
