namespace GameApi.Services;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }

    public static Result Success()
        => new Result { IsSuccess = true };

    public static Result Failure(string error)
        => new Result { IsSuccess = false, Error = error };
}
